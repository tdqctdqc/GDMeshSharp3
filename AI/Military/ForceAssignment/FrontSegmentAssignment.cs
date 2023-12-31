using System;
using System.Collections.Generic;
using MathNet.Numerics;
using System.Linq;
using Godot;
using MessagePack;

public class FrontSegmentAssignment : ForceAssignment
{
    public static int IdealSegmentLength = 5;
    public Vector2 Center { get; private set; }
    public List<int> LineWaypointIds { get; private set; }
    public Waypoint GetRallyWaypoint(Data d) 
        => MilitaryDomain.GetTacWaypoint(RallyWaypointId, d);
    public int RallyWaypointId { get; private set; }
    public static FrontSegmentAssignment Construct(
        EntityRef<Regime> r,
        List<Waypoint> heldWaypoints,
        LogicWriteKey key)
    {
        var center = key.Data.Planet.GetAveragePosition(heldWaypoints.Select(wp => wp.Pos));
        var fsa = new FrontSegmentAssignment(key.Data.IdDispenser.TakeId(),
            heldWaypoints.Select(wp => wp.Id).ToList(),
            center,
            new HashSet<int>(),
            r,
            CalcRallyWaypoint(heldWaypoints, r.Entity(key.Data),
                key.Data).Id);
        return fsa;
    }
    [SerializationConstructor] private FrontSegmentAssignment(
        int id,
        List<int> lineWaypointIds,
        Vector2 center,
        HashSet<int> groupIds, 
        EntityRef<Regime> regime,
        int rallyWaypointId) 
        : base(groupIds, regime, id)
    {
        if (lineWaypointIds.Count == 0) throw new Exception();
        RallyWaypointId = rallyWaypointId;
        LineWaypointIds = lineWaypointIds;
        Center = center;
    }

    
    public override void CalculateOrders(MinorTurnOrders orders, LogicWriteKey key)
    {
        if (GroupIds.Count() == 0) return;
        var alliance = orders.Regime.Entity(key.Data).GetAlliance(key.Data);
        var areaRadius = 500f;
        var wps = GetTacWaypoints(key.Data).ToList();
        RallyWaypointId = 
            CalcRallyWaypoint(wps, Regime.Entity(key.Data), key.Data).Id;
        var rally = GetRallyWaypoint(key.Data);
        var moveType = key.Data.Models.MoveTypes.InfantryMove;
        if (moveType.Passable(rally, alliance, key.Data) == false)
        {
            throw new Exception();
        }
        var groups = Groups(key.Data).ToList();
        var lineGroups = GetLineGroups(key.Data);
        var unreadyGroups = groups.Except(lineGroups);
        
        if (lineGroups.Count() > 0)
        {
            Assigner.AssignAlongLine(GetTacWaypoints(key.Data).ToList(),
                lineGroups.ToList(),
                g => g.GetPowerPoints(key.Data),
                (v,w) => v.Pos.GetOffsetTo(w.Pos, key.Data).Length(),
                v => v.Pos,
                (v,w) => v.GetOffsetTo(w, key.Data),
                (g, l) =>
                {
                    var order = new DeployOnLineOrder(l, g.Units.RefIds.ToList());
                    var proc = new SetUnitOrderProcedure(g.MakeRef(), order);
                    key.SendMessage(proc);
                });
        }
        
        foreach (var unreadyGroup in unreadyGroups)
        {
            var pos = unreadyGroup.GetPosition(key.Data);
            var order = GoToWaypointOrder.Construct(
                rally, 
                unreadyGroup.Regime.Entity(key.Data),
                unreadyGroup, key.Data);
            if (order != null)
            {
                var proc = new SetUnitOrderProcedure(unreadyGroup.MakeRef(), order);
                key.SendMessage(proc);
            }
        }
    }

    public HashSet<UnitGroup> GetLineGroups(Data d)
    {
        var moveType = d.Models.MoveTypes.InfantryMove;
        var a = Regime.Entity(d).GetAlliance(d);
        var closeDist = 100f;
        var closeWps = GetRear(d, 3)
            .SelectMany(h => h)
            .Union(GetTacWaypoints(d))
            .Where(wp => moveType.Passable(wp, a, d))
            .ToHashSet();
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

    public static Waypoint CalcRallyWaypoint(IEnumerable<Waypoint> wps,
        Regime regime,
        Data d)
    {
        var radius = 3;
        var rings = GetRear(wps, regime, 3, d);
        var avgPos = d.Planet.GetAveragePosition(wps.Select(wp => wp.Pos));
        var moveType = d.Models.MoveTypes.InfantryMove;
        var alliance = regime.GetAlliance(d);

        if (rings.Count == 0)
        {
            return wps
                .Where(wp => moveType.Passable(wp, alliance, d))
                .MinBy(wp => wp.Pos.GetOffsetTo(avgPos, d).Length());
        }
        var rally = rings.Last()
            .MinBy(wp => wp.Pos.GetOffsetTo(avgPos, d).Length());
        if (moveType.Passable(rally, alliance, d) == false)
        {
            throw new Exception();
        }
        return rally;

    }

    public List<HashSet<Waypoint>> GetRear(Data d, int radius)
    {
        return GetRear(GetTacWaypoints(d),
            Regime.Entity(d),
            radius, d);
    }
    public static List<HashSet<Waypoint>> GetRear(IEnumerable<Waypoint> wps,
        Regime regime,
        int radius,
        Data data)
    {
        var moveType = data.Models.MoveTypes.InfantryMove;
        var alliance = regime.GetAlliance(data);
        var prev = wps.ToHashSet();
        var res = new List<HashSet<Waypoint>>();
        for (var i = 0; i < radius; i++)
        {
            var next = prev
                .SelectMany(r => r.TacNeighbors(data))
                .Where(p => prev.Contains(p) == false)
                .Where(r =>
                    wps.Contains(r) == false
                    && moveType.Passable(r, alliance, data)
                    && r.IsThreatened(alliance, data) == false
                    && r.IsControlled(alliance, data))
                .ToHashSet();
            if (next.Count == 0) break;
            prev.AddRange(next);
            res.Add(next);
        }

        return res;
    }
}