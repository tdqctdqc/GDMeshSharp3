using System;
using System.Collections.Generic;
using MathNet.Numerics;
using System.Linq;
using Godot;
using MessagePack;

public class FrontSegmentAssignment : ForceAssignment
{
    public static int IdealSegmentLength = 7;
    public Vector2 Center { get; private set; }
    public List<int> LineWaypointIds { get; private set; }
    public static FrontSegmentAssignment Construct(
        EntityRef<Regime> r,
        List<Waypoint> heldWaypoints,
        LogicWriteKey key)
    {
        var center = key.Data.Planet.GetAveragePosition(heldWaypoints.Select(wp => wp.Pos));
        
        return new FrontSegmentAssignment(key.Data.IdDispenser.TakeId(),
            heldWaypoints.Select(wp => wp.Id).ToList(),
            center,
            new HashSet<int>(),
            r);
    }
    [SerializationConstructor] private FrontSegmentAssignment(
        int id,
        List<int> lineWaypointIds,
        Vector2 center,
        HashSet<int> groupIds, 
        EntityRef<Regime> regime) 
        : base(groupIds, regime, id)
    {
        LineWaypointIds = lineWaypointIds;
        Center = center;
    }

    
    public override void CalculateOrders(MinorTurnOrders orders, LogicWriteKey key)
    {
        if (GroupIds.Count() == 0) return;
        var alliance = orders.Regime.Entity(key.Data).GetAlliance(key.Data);
        var areaRadius = 500f;
        var wps = GetTacWaypoints(key.Data).ToList();
        
        var groups = Groups(key.Data).ToList();
        var lineGroups = GetLineGroups(key.Data);
        var unreadyGroups = groups.Except(lineGroups);
        if (lineGroups.Count() > 0)
        {
            Assigner.AssignAlongLine(GetTacWaypoints(key.Data).ToList(),
                lineGroups.ToList(),
                g => g.GetPowerPoints(key.Data),
                (v,w) => v.Pos.GetOffsetTo(w.Pos, key.Data).Length(),
                v => GetWpDeployPos(v, alliance, key.Data),
                (v,w) => v.GetOffsetTo(w, key.Data),
                (g, l) =>
                {
                    var order = new DeployOnLineOrder(l);
                    var proc = new SetUnitOrderProcedure(g.MakeRef(), order);
                    key.SendMessage(proc);
                });
        }
        
        foreach (var unreadyGroup in unreadyGroups)
        {
            var pos = unreadyGroup.GetPosition(key.Data);
            var close = wps
                .Where(wp => PathFinder.IsLandPassable(wp, alliance, key.Data))
                .MinBy(wp => wp.Pos.GetOffsetTo(pos, key.Data).Length());
            var order = GoToWaypointOrder.Construct(close, 
                unreadyGroup.Regime.Entity(key.Data),
                unreadyGroup, key.Data);
            if (order != null)
            {
                var proc = new SetUnitOrderProcedure(unreadyGroup.MakeRef(), order);
                key.SendMessage(proc);
            }
        }
    }

    private Vector2 GetWpDeployPos(Waypoint wp, Alliance a, Data d)
    {
        if (wp is IRiverWaypoint)
        {
            return wp.TacNeighbors(d)
                .Where(n => n is ILandWaypoint
                            && n.IsControlled(a, d))
                .Select(n => wp.Pos.GetOffsetTo(n.Pos, d).Normalized())
                .Sum().Normalized() * 20f + wp.Pos;
        }

        return wp.Pos;
    }
    public HashSet<UnitGroup> GetLineGroups(Data d)
    {
        var a = Regime.Entity(d).GetAlliance(d);
        var closeDist = 100f;
        var closeWps = GetTacWaypoints(d)
            .SelectMany(wp => wp.TacNeighbors(d))
            .Where(wp => PathFinder.IsLandPassable(wp, a, d))
            .ToHashSet();
        var context = d.Context;
        var readyGroups = Groups(d)
            .Where(g =>
            {
                var pos = g.GetPosition(d);
                var dist = closeWps.Min(wp => wp.Pos.GetOffsetTo(pos, d).Length());
                return dist < closeDist;
            });
        
        return readyGroups.ToHashSet();
    }
    public IEnumerable<Waypoint> GetTacWaypoints(Data d)
    {
        return LineWaypointIds.Select(id => MilitaryDomain.GetTacWaypoint(id, d));
    }
    public override float GetPowerPointNeed(Data data)
    {
        var opposing = GetOpposingPowerPoints(data);
        var length = GetLength(data);

        return opposing * FrontAssignment.CoverOpposingWeight
               + length * FrontAssignment.CoverLengthWeight * FrontAssignment.PowerPointsPerLengthToCover;
    }
    public float GetOpposingPowerPoints(Data data)
    {
        var forceBalances = data.Context.WaypointForceBalances;
        var alliance = Regime.Entity(data).GetAlliance(data);
        return GetTacWaypoints(data)
            .Sum(wp => forceBalances[wp].GetHostilePowerPoints(alliance, data));
    }
    
    public void MergeInto(FrontSegmentAssignment merging, LogicWriteKey key)
    {
        GroupIds.AddRange(merging.GroupIds);
    }

    public override UnitGroup RequestGroup(LogicWriteKey key)
    {
        if (GroupIds.Count < 2) return null;
        int de = GroupIds.First();
        GroupIds.Remove(de);
        return key.Data.Get<UnitGroup>(de);
    }

    public override void TakeAwayGroup(UnitGroup g, LogicWriteKey key)
    {
        GroupIds.Remove(g.Id);
    }

    public override void AssignGroups(LogicWriteKey key)
    {
        GetLineGroups(key.Data);
    }

    public float GetLength(Data d)
    {
        var length = 0f;
        for (var i = 0; i < LineWaypointIds.Count - 1; i++)
        {
            var from = MilitaryDomain.GetTacWaypoint(LineWaypointIds[i], d);
            var to = MilitaryDomain.GetTacWaypoint(LineWaypointIds[i + 1], d);
            length += from.Pos.GetOffsetTo(to.Pos, d).Length();
        }

        return length;
    }
}