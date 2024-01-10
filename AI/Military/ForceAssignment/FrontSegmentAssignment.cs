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
    public List<int> FrontLineWpIds { get; private set; }
    public List<int> AdvanceLineWpIds { get; private set; }
    public bool Attack { get; private set; }
    public Waypoint GetRallyWaypoint(Data d) 
        => MilitaryDomain.GetTacWaypoint(RallyWaypointId, d);
    public int RallyWaypointId { get; private set; }
    public static FrontSegmentAssignment Construct(
        EntityRef<Regime> r,
        List<Waypoint> frontLine,
        List<Waypoint> advanceLine,
        bool attack,
        LogicWriteKey key)
    {
        var center = key.Data.Planet.GetAveragePosition(frontLine.Select(wp => wp.Pos));
        var frontLineIds = frontLine.Select(wp => wp.Id).ToList();
        var advanceLineIds = advanceLine?.Select(wp => wp.Id).ToList();
        var fsa = new FrontSegmentAssignment(
            key.Data.IdDispenser.TakeId(),
            frontLineIds,
            advanceLineIds,
            center,
            new HashSet<int>(),
            r,
            CalcRallyWaypoint(frontLine, r.Entity(key.Data),
                key.Data).Id,
            attack);
        return fsa;
    }
    [SerializationConstructor] private FrontSegmentAssignment(
        int id,
        List<int> frontLineWpIds,
        List<int> advanceLineWpIds,
        Vector2 center,
        HashSet<int> groupIds, 
        EntityRef<Regime> regime,
        int rallyWaypointId,
        bool attack) 
        : base(groupIds, regime, id)
    {
        if (frontLineWpIds.Count == 0) throw new Exception();
        Attack = attack;
        RallyWaypointId = rallyWaypointId;
        FrontLineWpIds = frontLineWpIds;
        AdvanceLineWpIds = advanceLineWpIds;
        Center = center;
    }
    
    public override void CalculateOrders(MinorTurnOrders orders,
        LogicWriteKey key)
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
        var readyGroups = GetReadyGroups(key.Data);
        
        var unreadyGroups = groups
            .Except(readyGroups).ToHashSet();
        var frontLength = GetLength(key.Data);
        var readyGroupFrontage = readyGroups.Sum(g => g.Units.Items(key.Data).Sum(u => u.Radius() * 2f));
        var frontageToTake = Mathf.Min(frontLength, readyGroupFrontage);

        var advanceLine = AdvanceLineWpIds
            ?.Select(id => MilitaryDomain.GetTacWaypoint(id, key.Data).Pos)
            .ToList();
        
        if (readyGroups.Count() > 0)
        {
            var assgns = Assigner.PickBestAndAssignAlongLine(
                GetTacWaypoints(key.Data).ToList(),
                readyGroups.ToList(),
                g => g.Units.Items(key.Data).Sum(u => u.Radius() * 2f),
                frontageToTake,
                (v,w) => v.Pos.GetOffsetTo(w.Pos, key.Data).Length(),
                v => v.Pos,
                g => g.GetPosition(key.Data),
                (v,w) => v.GetOffsetTo(w, key.Data)
            );
            foreach (var readyGroup in readyGroups)
            {
                if (assgns.ContainsKey(readyGroup))
                {
                    var assgn = assgns[readyGroup];
        
                    List<Vector2> advanceSubLine;
                    if (advanceLine != null)
                    {
                        advanceSubLine = advanceLine.GetSubline((v, w) => v.GetOffsetTo(w, key.Data),
                            assgn.FromProportion, assgn.ToProportion);
                    }
                    else
                    {
                        advanceSubLine = null;
                    }
                    var order = new DeployOnLineOrder(assgn.SubLine, 
                        advanceSubLine,
                        readyGroup.Units.RefIds.ToList(), 
                        true,
                        Attack
                    );
                    var proc = new SetUnitOrderProcedure(readyGroup.MakeRef(), order);
                    key.SendMessage(proc);
                }
                else
                {
                    unreadyGroups.Add(readyGroup);
                }
            }
        }
        
        foreach (var unreadyGroup in unreadyGroups)
        {
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

    public HashSet<UnitGroup> GetReadyGroups(Data d)
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
        return FrontLineWpIds.Select(id => MilitaryDomain.GetTacWaypoint(id, d));
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
    }

    public float GetLength(Data d)
    {
        var length = 0f;
        for (var i = 0; i < FrontLineWpIds.Count - 1; i++)
        {
            var from = MilitaryDomain.GetTacWaypoint(FrontLineWpIds[i], d);
            var to = MilitaryDomain.GetTacWaypoint(FrontLineWpIds[i + 1], d);
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
                .SelectMany(r => r.GetNeighbors(data))
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