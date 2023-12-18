
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TheaterAssignment : ForceAssignment
{
    
    public List<FrontAssignment> Fronts { get; private set; }
    public HashSet<int> TacWaypointIds { get; private set; }
    public TheaterAssignment(int id, EntityRef<Regime> regime, 
        List<FrontAssignment> fronts,
        HashSet<int> tacWaypointIds, HashSet<int> groupIds) 
        : base(groupIds, regime, id)
    {
        Fronts = fronts;
        TacWaypointIds = tacWaypointIds;
    }
    public override void CalculateOrders(MinorTurnOrders orders, LogicWriteKey key)
    {
        foreach (var fa in Fronts)
        {
            fa.CalculateOrders(orders, key);
        }
    }

    public override float GetPowerPointNeed(Data d)
    {
        return Fronts.Sum(fa => fa.GetPowerPointNeed(d));
    }

    public IEnumerable<Waypoint> GetTacWaypoints(Data d)
    {
        return TacWaypointIds.Select(id => MilitaryDomain.GetTacWaypoint(id, d));
    }
    public static void CheckSplitRemove(Regime r, List<TheaterAssignment> theaters, 
        Action<ForceAssignment> add, Action<ForceAssignment> remove, LogicWriteKey key)
    {
        var alliance = r.GetAlliance(key.Data);
        
        var controlledWps = key.Data.Context.ControlledAreas[alliance];
        for (var i = 0; i < theaters.Count; i++)
        {
            var ta = theaters[i];
            ta.TacWaypointIds.RemoveWhere(i => controlledWps.Contains(MilitaryDomain.GetTacWaypoint(i, key.Data)) == false);
            
            if (ta.TacWaypointIds.Count == 0)
            {
                remove(ta);
                return;
            }
            
            var flood = FloodFill<Waypoint>.GetFloodFill(ta.GetTacWaypoints(key.Data).First(),
                wp => ta.TacWaypointIds.Contains(wp.Id),
                wp => wp.TacNeighbors(key.Data));

            if (flood.Count() != ta.TacWaypointIds.Count)
            {
                remove(ta);
                var newTheaters = Divide(ta, key);
                foreach (var newTa in newTheaters)
                {
                    add(newTa);
                }
            }
        }
    }
    
    public static void CheckExpandMergeNew(Regime r, List<TheaterAssignment> theaters, 
        Action<ForceAssignment> add,
        Action<ForceAssignment> remove,
        LogicWriteKey key)
    {
        var alliance = r.GetAlliance(key.Data);
        var allianceAi = key.Data.HostLogicData.AllianceAis[alliance];
        var responsibility = allianceAi
            .MilitaryAi.AreasOfResponsibility[r].ToHashSet();
        var responsibilityIds = responsibility.Select(r => r.Id).ToHashSet();
        
        var covered = new HashSet<int>();
        var toMerge = new Dictionary<HashSet<int>, List<TheaterAssignment>>();
        foreach (var ta in theaters)
        {
            var first = ta.TacWaypointIds.First();
            var dissolve = false;
            if (covered.Contains(first))
            {
                var k = toMerge.First(kvp => kvp.Key.Contains(first)).Key;
                toMerge[k].Add(ta);
            }
            else
            {
                var flood = FloodFill<int>.GetFloodFill(
                    first, responsibilityIds.Contains,
                    t => MilitaryDomain.GetTacWaypoint(t, key.Data).Neighbors);
                covered.AddRange(flood);
                toMerge.Add(flood, new List<TheaterAssignment>{ta});
                ta.TacWaypointIds.AddRange(flood);
            }
        }
        
        foreach (var kvp in toMerge)
        {
            var list = kvp.Value;
            if (list.Count == 1)
            {
                continue;
            }
            
            var into = list[0];
            for (var i = 1; i < list.Count; i++)
            {
                into.MergeInto(list[i]);
                remove(list[i]);
            }
        }

        var uncovered = responsibility.Except(covered
            .Select(c => MilitaryDomain.GetTacWaypoint(c, key.Data))).ToList();
        var uncoveredUnions = UnionFind.Find(uncovered, (w, v) => true,
            w => w.TacNeighbors(key.Data));
        foreach (var union in uncoveredUnions)
        {
            var ta = new TheaterAssignment(key.Data.IdDispenser.TakeId(), r.MakeRef(), new List<FrontAssignment>(),
                union.Select(u => u.Id).ToHashSet(), new HashSet<int>());
            add(ta);
        }
    }

    
    public void MergeInto(TheaterAssignment dissolve)
    {
        GroupIds.AddRange(dissolve.GroupIds);
        Fronts.AddRange(dissolve.Fronts);
    }

    public static IEnumerable<TheaterAssignment> Divide(TheaterAssignment ta, LogicWriteKey key)
    {
        var r = ta.Regime.Entity(key.Data);
        var unions = UnionFind.Find(ta.TacWaypointIds,
            (i, j) => true,
            i => MilitaryDomain.GetTacWaypoint(i, key.Data).Neighbors);
        
        var newTheaters =
            unions.Select(
                u => new TheaterAssignment(key.Data.IdDispenser.TakeId(), r.MakeRef(), new List<FrontAssignment>(),
                    u.ToHashSet(), new HashSet<int>()));
        foreach (var group in ta.Groups(key.Data))
        {
            var unitWpIds = group.Units.Items(key.Data)
                .Select(u => key.Data.Context.UnitWaypoints[u].Id).ToList();
            var mostWpsShared = newTheaters
                .MaxBy(t => unitWpIds.Where(t.TacWaypointIds.Contains).Count());
            if (unitWpIds.Where(mostWpsShared.TacWaypointIds.Contains).Count() == 0)
            {
                var pos = group.GetPosition(key.Data);
                var closest = newTheaters
                    .MinBy(t => 
                        group.GetPosition(key.Data).GetOffsetTo(
                        key.Data.Planet.GetAveragePosition(t.TacWaypointIds.Select(i =>
                            MilitaryDomain.GetTacWaypoint(i, key.Data).Pos)),
                        key.Data)
                    );
                closest.GroupIds.Add(group.Id);
            }
            else
            {
                mostWpsShared.GroupIds.Add(group.Id);
            }
        }

        return newTheaters;
    }
    
    public static void CheckFronts(Regime r, 
        List<TheaterAssignment> theaters, 
        LogicWriteKey key)
    {
        var alliance = r.GetAlliance(key.Data);
        foreach (var ta in theaters)
        {
            FrontAssignment.CheckSplitRemove(r, ta, ta.Fronts.ToList(),
                ta.Fronts.Add, f => ta.Fronts.Remove(f),
                key);
            FrontAssignment.CheckExpandMergeNew(r, ta, ta.Fronts.ToList(),
                ta.Fronts.Add, f => ta.Fronts.Remove(f),
                key);
            foreach (var fa in ta.Fronts)
            {
                fa.CheckSegments(key);
            }
        }
    }
    public void AssignGroups(LogicWriteKey key)
    {
        ShiftGroups(key);
        AssignFreeGroups(key);
        foreach (var fa in Fronts)
        {
            fa.AssignGroups(key);
        }
    }

    private void AssignFreeGroups(LogicWriteKey key)
    {
        var data = key.Data;
        var regime = Regime.Entity(data);
        var totalLength = Fronts.Sum(fa => fa.TacWaypointIds.Count);
        var totalOpposing = Fronts.Sum(fa => fa.GetOpposingPowerPoints(data));
        
        var occupiedGroups = Fronts
            .SelectMany(fa => fa.GroupIds)
            .Select(g => data.Get<UnitGroup>(g));
        
        //todo make specific to theater
        var freeGroups = data.Military.UnitAux.UnitGroupByRegime[regime]
            ?.Except(occupiedGroups)
            ?.ToList();
        if (freeGroups == null || freeGroups.Count == 0) return;
        
        Assigner.Assign<FrontAssignment, UnitGroup>(
            Fronts,
            fa => fa.GetPowerPointNeed(data),
            fa => fa.Groups(data),
            g => g.GetPowerPoints(data),
            freeGroups.ToHashSet(),
            (fa, g) => fa.GroupIds.Add(g.Id),
            (fa, g) => g.GetPowerPoints(data));
    }

    private void ShiftGroups(LogicWriteKey key)
    {
        if (Fronts.Count < 2) return;
        var data = key.Data;

        var max = maxSatisfied();
        var min = minSatisfied();
        var iter = 0;
        
        while (iter < Fronts.Count * 2 
               && max.ratio > min.ratio * 1.5f)
        {
            var g = max.fa.DeassignGroup(key);
            if (g != null)
            {
                min.fa.GroupIds.Add(g.Id);
            }
            max = maxSatisfied();
            min = minSatisfied();
            iter++;
        }

        (float ratio, FrontAssignment fa) maxSatisfied()
        {
            var max = Fronts.MaxBy(fa => fa.GetSatisfiedRatio(data));
            return (max.GetSatisfiedRatio(data), max);
        }

        (float ratio, FrontAssignment fa) minSatisfied()
        {
            var min = Fronts.MinBy(fa => fa.GetSatisfiedRatio(data));
            return (min.GetSatisfiedRatio(data), min);
        }
    }
}