
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TheaterAssignment : ForceAssignment
{
    
    public List<FrontAssignment> Fronts { get; private set; }
    public HashSet<int> TacWaypoints { get; private set; }
    public TheaterAssignment(EntityRef<Regime> regime, 
        List<FrontAssignment> fronts,
        HashSet<int> tacWaypoints, HashSet<int> groupIds) 
        : base(groupIds, regime)
    {
        Fronts = fronts;
        TacWaypoints = tacWaypoints;
    }
    public override void CalculateOrders(MinorTurnOrders orders, LogicWriteKey key)
    {
        foreach (var fa in Fronts)
        {
            fa.CalculateOrders(orders, key);
        }
    }

    public static void CheckSplitRemove(Regime r, List<TheaterAssignment> theaters, 
        Action<ForceAssignment> remove,  LogicWriteKey key)
    {
        var alliance = r.GetAlliance(key.Data);
        var allianceAi = key.Data.HostLogicData.AllianceAis[alliance];
        

        var control = key.Data.Context.ControlledAreas[alliance];
        for (var i = 0; i < theaters.Count; i++)
        {
            var ta = theaters[i];
            ta.TacWaypoints.RemoveWhere(i => control.Contains(key.Data.Military.TacticalWaypoints.Waypoints[i]) == false);
            if (ta.TacWaypoints.Count == 0)
            {
                remove(ta);
                return;
            }
            var unions = UnionFind.Find(ta.TacWaypoints,
                (i, j) => true,
                i => key.Data.Military.TacticalWaypoints.Waypoints[i].Neighbors);
            if (unions.Count() == 0) throw new Exception();
            if (unions.Count > 1)
            {
                GD.Print("no split yet");
                remove(ta);
                for (var j = 0; j < unions.Count; j++)
                {
                    var poly = key.Data.Military.TacticalWaypoints
                        .Waypoints[unions[j].First()].AssocPolys(key.Data).First();
                    GD.Print($"split {j} at {poly.Id}");
                }
            }
        }
        
    }
    
    public static void CheckExpandMergeNew(Regime r, List<TheaterAssignment> theaters, 
        Action<ForceAssignment> remove,
        Action<ForceAssignment> add,
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
            var first = ta.TacWaypoints.First();
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
                    t => key.Data.Military.TacticalWaypoints.Waypoints[t].Neighbors);
                covered.AddRange(flood);
                toMerge.Add(flood, new List<TheaterAssignment>{ta});
                ta.TacWaypoints.AddRange(flood);
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

        var uncovered = responsibility.Except(covered.Select(c => key.Data.Military.TacticalWaypoints.Waypoints[c]));
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
        foreach (var ta in theaters)
        {
            ta.Fronts.Clear();
            var contactLines = DeploymentAi.GetContactLines(r,
                ta.TacWaypoints.Select(id => key.Data.Military.TacticalWaypoints.Waypoints[id]).ToHashSet(),
                key.Data);
            foreach (var contactLine in contactLines)
            {
                var front = Front.Construct(r,
                    contactLine.Select(wp => wp.Id).ToList(),
                    key);
                var fa = new FrontAssignment(front, new HashSet<int>());
                ta.Fronts.Add(fa);
            }
        }
        // throw new NotImplementedException();
    }
    public void MergeInto(TheaterAssignment dissolve)
    {
        GroupIds.AddRange(dissolve.GroupIds);
        Fronts.AddRange(dissolve.Fronts);
    }
}