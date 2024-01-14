
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TheaterAssignment : ForceAssignment, ICompoundForceAssignment
{
    public HashSet<ForceAssignment> Assignments { get; private set; }
    public HashSet<int> TacWaypointIds { get; private set; }
    public TheaterAssignment(int id, EntityRef<Regime> regime, 
        HashSet<ForceAssignment> assignments,
        HashSet<int> tacWaypointIds, HashSet<int> groupIds) 
        : base(groupIds, regime, id)
    {
        Assignments = assignments;
        TacWaypointIds = tacWaypointIds;
    }
    public override void CalculateOrders(MinorTurnOrders orders, LogicWriteKey key)
    {
        foreach (var fa in Assignments)
        {
            fa.CalculateOrders(orders, key);
        }
    }

    public override float GetPowerPointNeed(Data d)
    {
        return Assignments.Sum(fa => fa.GetPowerPointNeed(d));
    }

    public IEnumerable<Waypoint> GetWaypoints(Data d)
    {
        return TacWaypointIds.Select(id => MilitaryDomain.GetWaypoint(id, d));
    }
    public static void CheckSplitRemove(Regime r, List<TheaterAssignment> theaters, 
        Action<ForceAssignment> add, Action<ForceAssignment> remove, LogicWriteKey key)
    {
        var alliance = r.GetAlliance(key.Data);
        
        var controlledWps = key.Data.Context
            .ControlledAreas[alliance];
        for (var i = 0; i < theaters.Count; i++)
        {
            var ta = theaters[i];
            ta.TacWaypointIds.RemoveWhere(i => controlledWps.Contains(MilitaryDomain.GetWaypoint(i, key.Data)) == false);
            
            if (ta.TacWaypointIds.Count == 0)
            {
                remove(ta);
                return;
            }
            
            var flood = FloodFill<Waypoint>.GetFloodFill(ta.GetWaypoints(key.Data).First(),
                wp => ta.TacWaypointIds.Contains(wp.Id),
                wp => wp.GetNeighbors(key.Data));

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
            if (covered.Contains(first))
            {
                var k = toMerge.First(kvp => kvp.Key.Contains(first)).Key;
                toMerge[k].Add(ta);
            }
            else
            {
                var flood = FloodFill<int>.GetFloodFill(
                    first, responsibilityIds.Contains,
                    t => MilitaryDomain.GetWaypoint(t, key.Data).Neighbors);
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
            .Select(c => MilitaryDomain.GetWaypoint(c, key.Data))).ToList();
        var uncoveredUnions = UnionFind.Find(uncovered, (w, v) => true,
            w => w.GetNeighbors(key.Data));
        foreach (var union in uncoveredUnions)
        {
            var ta = new TheaterAssignment(key.Data.IdDispenser.TakeId(), r.MakeRef(), new HashSet<ForceAssignment>(),
                union.Select(u => u.Id).ToHashSet(), new HashSet<int>());
            add(ta);
        }
    }

    public static void PutGroupsInRightTheater(Regime r, 
        IEnumerable<ForceAssignment> forceAssignments, 
        LogicWriteKey key)
    {
        var groups = key.Data.Military.UnitAux.UnitGroupByRegime[r];
        var freeGroups = groups
            .Where(g => forceAssignments.Any(fa => fa.GroupIds.Contains(fa.Id) == false))
            .ToHashSet();
        var claimedGroups = groups.Except(freeGroups).ToHashSet();
        foreach (var freeGroup in freeGroups)
        {
            var wps = freeGroup.Units.Items(key.Data)
                .Select(u => key.Data.Context.UnitWaypoints[u]).ToHashSet();
            var theater = forceAssignments.OfType<TheaterAssignment>()
                .FirstOrDefault(t => wps.Any(wp => t.TacWaypointIds.Contains(wp.Id)));
            if (theater == null)
            {
                GD.Print($"couldnt find theater for free unit group " +
                         $"of {r.Name} " +
                         $"at wp {wps.First().Id}");
                continue;
            }
            theater.GroupIds.Add(freeGroup.Id);
        }
        
        foreach (var claimedGroup in claimedGroups)
        {
            var wps = claimedGroup.Units.Items(key.Data)
                .Select(u => key.Data.Context.UnitWaypoints[u]).ToHashSet();
            var theatersClaiming = forceAssignments.OfType<TheaterAssignment>()
                .Where(t => t.GroupIds.Contains(claimedGroup.Id));
            if (theatersClaiming.Count() > 1)
            {
                GD.Print("multiple theaters claiming unit group");
                throw new Exception();
            }
            var theaterClaiming = theatersClaiming.First();
            if (wps.Any(wp => theaterClaiming.TacWaypointIds.Contains(wp.Id)) == false)
            {
                GD.Print("theater doesnt have wp for claimed unit group");
                var theatersSharingWp = forceAssignments
                    .OfType<TheaterAssignment>()
                    .Where(ta => wps.Any(wp => ta.TacWaypointIds.Contains(wp.Id)));
                if (theatersSharingWp.Count() == 0)
                {
                    GD.Print("couldnt find theater for claimed unit group");
                    continue;
                }
                theaterClaiming.TakeAwayGroup(claimedGroup, key);
                theatersSharingWp.First().GroupIds.Add(claimedGroup.Id);
            }
        }
    }
    public void SetTargets(LogicWriteKey key)
    {
        foreach (var front in Assignments.OfType<FrontAssignment>())
        {
            front.SetTargets(key);
        }
    }
    public void MergeInto(TheaterAssignment dissolve)
    {
        GroupIds.AddRange(dissolve.GroupIds);
        Assignments.AddRange(dissolve.Assignments);
    }

    public static IEnumerable<TheaterAssignment> Divide(TheaterAssignment ta, LogicWriteKey key)
    {
        var r = ta.Regime.Entity(key.Data);
        var unions = UnionFind.Find(ta.TacWaypointIds,
            (i, j) => true,
            i => MilitaryDomain.GetWaypoint(i, key.Data).Neighbors);
        
        var newTheaters =
            unions.Select(
                u => new TheaterAssignment(key.Data.IdDispenser.TakeId(), r.MakeRef(), new HashSet<ForceAssignment>(),
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
                            MilitaryDomain.GetWaypoint(i, key.Data).Pos)),
                        key.Data).Length()
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
            var fronts = ta.Assignments.OfType<FrontAssignment>();
            foreach (var fa in fronts)
            {
                ta.Assignments.Remove(fa);
            }
            var newFronts = fronts.Blob(ta, key);
            ta.Assignments.AddRange(newFronts);
            foreach (var fa in ta.Assignments.OfType<FrontAssignment>())
            {
                fa.CheckSegments(key);
            }
        }
    }
    public override void AssignGroups(LogicWriteKey key)
    {
        this.ShiftGroups(key);
        this.AssignFreeGroups(key);
        foreach (var fa in Assignments)
        {
            fa.AssignGroups(key);
        }
    }

    public override UnitGroup RequestGroup(LogicWriteKey key)
    {
        if (GroupIds.Count < 2) return null;
        UnitGroup deassign = deassign = Assignments
            .MaxBy(s => s.GetSatisfiedRatio(key.Data))
            .RequestGroup(key);
        
        if(deassign == null)
        {
            deassign = key.Data.Get<UnitGroup>(GroupIds.First());
        }
        
        if(deassign != null) GroupIds.Remove(deassign.Id);
        return deassign;
    }
    
    public override void TakeAwayGroup(UnitGroup g, LogicWriteKey key)
    {
        this.TakeAwayGroupCompound(g, key);
    }
    
    public override Waypoint GetCharacteristicWaypoint(Data d)
    {
        return GetWaypoints(d)
            .FirstOrDefault(wp => wp.GetOccupyingRegime(d).Id == Regime.RefId);
    }
}