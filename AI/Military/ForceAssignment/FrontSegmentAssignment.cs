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
    public List<int> FrontLineCellIds { get; private set; }
    public List<int> AdvanceLineCellIds { get; private set; }
    public bool Attack { get; private set; }
    public PolyCell GetRallyCell(Data d) 
        => PlanetDomainExt.GetPolyCell(RallyWaypointId, d);
    public int RallyWaypointId { get; private set; }
    public static FrontSegmentAssignment Construct(
        EntityRef<Regime> r,
        List<PolyCell> frontLine,
        List<PolyCell> advanceLine,
        bool attack,
        LogicWriteKey key)
    {
        var center = key.Data.Planet.GetAveragePosition(frontLine.Select(wp => wp.GetCenter()));
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
        List<int> frontLineCellIds,
        List<int> advanceLineCellIds,
        Vector2 center,
        HashSet<int> groupIds, 
        EntityRef<Regime> regime,
        int rallyWaypointId,
        bool attack) 
        : base(groupIds, regime, id)
    {
        if (frontLineCellIds.Count == 0) throw new Exception();
        Attack = attack;
        RallyWaypointId = rallyWaypointId;
        FrontLineCellIds = frontLineCellIds;
        AdvanceLineCellIds = advanceLineCellIds;
        Center = center;
    }
    
    public override void CalculateOrders(MinorTurnOrders orders,
        LogicWriteKey key)
    {
        if (GroupIds.Count() == 0) return;
        var alliance = orders.Regime.Entity(key.Data).GetAlliance(key.Data);
        var areaRadius = 500f;
        var wps = GetCells(key.Data).ToList();
        RallyWaypointId = 
            CalcRallyWaypoint(wps, Regime.Entity(key.Data), key.Data).Id;
        var rally = GetRallyCell(key.Data);
        var moveType = key.Data.Models.MoveTypes.InfantryMove;
        if (moveType.Passable(rally, alliance, key.Data) == false)
        {
            throw new Exception();
        }
        var groups = Groups(key.Data).ToList();
        var readyGroups = 
            // groups.ToHashSet();
            GetReadyGroups(key.Data);
        
        var unreadyGroups = groups
            .Except(readyGroups).ToHashSet();
        var frontLength = GetLength(key.Data);
        var readyGroupFrontage = readyGroups.Sum(g => g.Units.Items(key.Data).Sum(u => u.Radius() * 2f));
        
        // var frontageToTake = Mathf.Min(frontLength, readyGroupFrontage);
        var frontageToTake = readyGroupFrontage;
        
        
        var advanceLine = AdvanceLineCellIds
            ?.Select(id => PlanetDomainExt.GetPolyCell(id, key.Data).GetCenter())
            .ToList();
        
        if (readyGroups.Count() > 0)
        {
            var assgns = Assigner.PickBestAndAssignAlongLine(
                GetCells(key.Data).ToList(),
                readyGroups.ToList(),
                g => g.Units.Items(key.Data).Sum(u => u.Radius() * 2f),
                frontageToTake,
                (v,w) => v.GetCenter().GetOffsetTo(w.GetCenter(), key.Data).Length(),
                v => v.GetCenter(),
                g => g.GetPosition(key.Data),
                (v,w) => v.GetOffsetTo(w, key.Data)
            );
            foreach (var readyGroup in readyGroups)
            {
                if (assgns.ContainsKey(readyGroup) == false)
                {
                    unreadyGroups.Add(readyGroup);
                    continue;
                }
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
                var order = new DeployOnLineGroupOrder(assgn.SubLine, 
                    advanceSubLine,
                    readyGroup.Units.RefIds.ToList(), 
                    Attack,
                    RallyWaypointId
                );
                var proc = new SetUnitOrderProcedure(readyGroup.MakeRef(), order);
                key.SendMessage(proc);
            }
        }
        
        foreach (var unreadyGroup in unreadyGroups)
        {
            var order = GoToCellGroupOrder.Construct(
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
        var closeDist = 500f;
        var closeWps = GetRear(d, 3)
            .SelectMany(h => h)
            .Union(GetCells(d))
            .Where(wp => moveType.Passable(wp, a, d))
            .ToHashSet();
        var readyGroups = Groups(d)
            .Where(g =>
            {
                var pos = g.GetPosition(d);
                var dist = closeWps.Min(wp => wp.GetCenter().GetOffsetTo(pos, d).Length());
                return dist < closeDist;
            });
        return readyGroups.ToHashSet();
    }
    public IEnumerable<PolyCell> GetCells(Data d)
    {
        return FrontLineCellIds.Select(id => PlanetDomainExt.GetPolyCell(id, d));
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
        var alliance = Regime.Entity(data).GetAlliance(data);
        return GetCells(data)
            .SelectMany(c => c.GetNeighbors(data))
            .Distinct()
            .Where(n => n.RivalControlled(alliance, data))
            .Sum(n => data.Context.UnitsByCell[n].Sum(u => u.GetPowerPoints(data)));
    }
    
    public void MergeInto(FrontSegmentAssignment merging, LogicWriteKey key)
    {
        GroupIds.AddRange(merging.GroupIds);
    }

    public void SetAdvance(bool attack, LogicWriteKey key)
    {
        // Attack = attack;
        // AdvanceLineCellIds = null;
        // var line = new LinkedList<int>();
        // var takenHash = new HashSet<int>();
        // var alliance = Regime.Entity(key.Data).GetAlliance(key.Data);
        // var wps = GetCells(key.Data).ToHashSet();
        // var hostiles = wps
        //     .SelectMany(wp => wp.GetNeighbors(key.Data))
        //     .Where(n => n.IsHostile(alliance, key.Data))
        //     .ToHashSet();
        // if (hostiles.Count == 0) return;
        //
        // var closest = hostiles.MinBy(h => 
        //     wps.Min(wp => wp.Pos
        //         .GetOffsetTo(h.Pos, key.Data).Length()));
        //
        // takenHash.Add(closest.Id);
        // line.AddFirst(closest.Id);
        // expand(closest, true);
        // expand(closest, false);
        // if (line.Count < 2) return;
        // AdvanceLineCellIds = line.ToList();
        //
        // var firstAdvance = MilitaryDomain
        //     .GetWaypoint(AdvanceLineCellIds[0], key.Data);
        // var lastAdvance = MilitaryDomain
        //     .GetWaypoint(AdvanceLineCellIds[AdvanceLineCellIds.Count - 1], key.Data);
        // var firstInFrontLine = MilitaryDomain
        //     .GetWaypoint(FrontLineCellIds[0], key.Data);
        // var firstDist = firstAdvance.Pos.GetOffsetTo(firstInFrontLine.Pos, key.Data).Length();
        // var lastDist = lastAdvance.Pos.GetOffsetTo(firstInFrontLine.Pos, key.Data).Length();
        // if (firstDist > lastDist)
        // {
        //     AdvanceLineCellIds.Reverse();
        // }
        //
        // void expand(PolyCell hostile, bool right)
        // {
        //     var friendlies = hostile.GetNeighbors(key.Data)
        //         .Where(f => wps.Contains(f));
        //     var ns = hostile.GetNeighbors(key.Data)
        //         .Where(n => takenHash.Contains(n.Id) == false
        //                     && hostiles.Contains(n)
        //                     && n.GetNeighbors(key.Data).Any(nn => wps.Contains(nn)));
        //     if (ns.Count() == 0) return;
        //     var n = ns.MinBy(n => friendlies.Min(f => f.Pos.GetOffsetTo(n.Pos, key.Data).Length()));
        //     takenHash.Add(n.Id);
        //     if (right)
        //     {
        //         line.AddLast(n.Id);
        //     }
        //     else
        //     {
        //         line.AddFirst(n.Id);
        //     }
        //     expand(n, right);
        // }
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
        if(GroupIds.Count == 1)
        GroupIds.Remove(g.Id);
    }

    public override void AssignGroups(LogicWriteKey key)
    {
    }

    public float GetLength(Data d)
    {
        var length = 0f;
        for (var i = 0; i < FrontLineCellIds.Count - 1; i++)
        {
            var from = PlanetDomainExt.GetPolyCell(FrontLineCellIds[i], d);
            var to = PlanetDomainExt.GetPolyCell(FrontLineCellIds[i + 1], d);
            length += from.GetCenter().GetOffsetTo(to.GetCenter(), d).Length();
        }

        return length;
    }
    
    public static PolyCell CalcRallyWaypoint(
        IEnumerable<PolyCell> wps,
        Regime regime,
        Data d)
    {
        var radius = 3;
        var rings = GetRear(wps, regime, 3, d);
        var avgPos = d.Planet.GetAveragePosition(wps.Select(wp => wp.GetCenter()));
        var moveType = d.Models.MoveTypes.InfantryMove;
        var alliance = regime.GetAlliance(d);

        if (rings.Count == 0)
        {
            return wps
                .Where(wp => moveType.Passable(wp, alliance, d))
                .MinBy(wp => wp.GetCenter().GetOffsetTo(avgPos, d).Length());
        }
        var rally = rings.Last()
            .MinBy(wp => wp.GetCenter().GetOffsetTo(avgPos, d).Length());
        if (moveType.Passable(rally, alliance, d) == false)
        {
            throw new Exception();
        }
        return rally;
    }
    
    public List<HashSet<PolyCell>> GetRear(Data d, int radius)
    {
        return GetRear(GetCells(d),
            Regime.Entity(d),
            radius, d);
    }
    public static List<HashSet<PolyCell>> GetRear(IEnumerable<PolyCell> wps,
        Regime regime,
        int radius,
        Data data)
    {
        var moveType = data.Models.MoveTypes.InfantryMove;
        var alliance = regime.GetAlliance(data);
        var prev = wps.ToHashSet();
        var res = new List<HashSet<PolyCell>>();
        for (var i = 0; i < radius; i++)
        {
            var next = prev
                .SelectMany(r => r.GetNeighbors(data))
                .Where(p => prev.Contains(p) == false)
                .Where(r =>
                    wps.Contains(r) == false
                    && moveType.Passable(r, alliance, data)
                    && r.RivalControlled(alliance, data) == false
                    && r.Controlled(alliance, data))
                .ToHashSet();
            if (next.Count == 0) break;
            prev.AddRange(next);
            res.Add(next);
        }

        return res;
    }
    public override PolyCell GetCharacteristicCell(Data d)
    {
        return GetCells(d)
            .FirstOrDefault(wp => wp.Controller.RefId == Regime.RefId);
    }
}