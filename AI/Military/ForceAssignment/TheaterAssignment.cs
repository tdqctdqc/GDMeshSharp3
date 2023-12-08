
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TheaterAssignment : ForceAssignment
{
    
    public List<FrontAssignment> Fronts { get; private set; }
    public HashSet<int> TacWaypointIds { get; private set; }
    public TheaterAssignment(EntityRef<Regime> regime, 
        List<FrontAssignment> fronts,
        HashSet<int> tacWaypointIds, HashSet<int> groupIds) 
        : base(groupIds, regime)
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
        var responsibility = allianceAi.MilitaryAi.AreasOfResponsibility[r];
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

        var uncovered = responsibility.Except(covered.Select(c => MilitaryDomain.GetTacWaypoint(c, key.Data)));
        var uncoveredUnions = UnionFind.Find(uncovered, (w, v) => true,
            w => w.TacNeighbors(key.Data));
        foreach (var union in uncoveredUnions)
        {
            var ta = new TheaterAssignment(r.MakeRef(), new List<FrontAssignment>(),
                union.Select(u => u.Id).ToHashSet(), new HashSet<int>());
            add(ta);
        }
    }

    public static void CheckTheaterFronts(Regime r, 
        List<TheaterAssignment> theaters, 
        LogicWriteKey key)
    {
        var alliance = r.GetAlliance(key.Data);
        foreach (var ta in theaters)
        {
            ta.Fronts.Clear();
            var threatened = ta.TacWaypointIds
                .Select(i => MilitaryDomain.GetTacWaypoint(i, key.Data))
                .Where(wp => wp.IsDirectlyThreatened(alliance, key.Data)
                             || wp.IsIndirectlyThreatened(alliance, key.Data));

            var frontWpClouds = UnionFind.Find(threatened, (w, v) => true,
                w => w.TacNeighbors(key.Data));
            
            foreach (var contactLine in frontWpClouds)
            {
                var front = Front.Construct(r,
                    contactLine.Select(wp => wp.Id),
                    key);
                var fa = new FrontAssignment(front, new HashSet<int>(),
                    new List<FrontSegmentAssignment>());
                ta.Fronts.Add(fa);
            }
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
                u => new TheaterAssignment(r.MakeRef(), new List<FrontAssignment>(),
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
    // private void FillFronts(Regime regime, Data data)
    // {
    //     var totalLength = Fronts.Sum(fa => fa.Front.GetLength(data));
    //     var totalOpposing = Fronts.Sum(fa => fa.Front.GetOpposingPowerPoints(data));
    //     var coverLengthWeight = 1f;
    //     var coverOpposingWeight = 1f;
    //     var occupiedGroups = Fronts
    //         .SelectMany(fa => fa.GroupIds)
    //         .Select(g => data.Get<UnitGroup>(g));
    //     var freeGroups = data.Military.UnitAux.UnitGroupByRegime[regime]
    //         ?.Except(occupiedGroups)
    //         ?.ToList();
    //     if (freeGroups == null || freeGroups.Count == 0) return;
    //     
    //     Assigner.Assign<FrontAssignment, UnitGroup>(
    //         Fronts,
    //         fa => GetFrontDefenseNeed(fa, data, totalLength, coverLengthWeight, totalOpposing, coverOpposingWeight),
    //         g => g.GetPowerPoints(data),
    //         freeGroups.ToHashSet(),
    //         (fa, g) => fa.GroupIds.Add(g.Id),
    //         (fa, g) => g.GetPowerPoints(data));
    // }
}