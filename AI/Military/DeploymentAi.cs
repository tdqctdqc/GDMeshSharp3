using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class DeploymentAi
{
    public HashSet<ForceAssignment> ForceAssignments { get; private set; }
    public DeploymentAi()
    {
        ForceAssignments = new HashSet<ForceAssignment>();
    }
    public void Calculate(Regime regime, LogicWriteKey key, MinorTurnOrders orders)
    {
        TheaterAssignment.CheckSplitRemove(regime,
            ForceAssignments.SelectWhereOfType<TheaterAssignment>().ToList(),
            fa => ForceAssignments.Add(fa),
            fa => ForceAssignments.Remove(fa),
            key);
        TheaterAssignment.CheckExpandMergeNew(regime,
            ForceAssignments.SelectWhereOfType<TheaterAssignment>().ToList(),
            fa => ForceAssignments.Add(fa),
            fa => ForceAssignments.Remove(fa),
            key);
        TheaterAssignment.CheckFronts(regime, ForceAssignments.SelectWhereOfType<TheaterAssignment>().ToList(),
            key);
        foreach (var ta in ForceAssignments.SelectWhereOfType<TheaterAssignment>().ToList())
        {
            ta.AssignGroups(key);
        }
        
        foreach (var forceAssignment in ForceAssignments)
        {
            forceAssignment.CalculateOrders(orders, key);
        }
    }
    public IEnumerable<FrontAssignment> GetFrontAssignments()
    {
        return ForceAssignments.SelectWhereOfType<FrontAssignment>();
    }


    private static HashSet<(Waypoint wp1, Waypoint wp2)> GetEdgesWithin(HashSet<Waypoint> wps, Vector2 relTo, Data data)
    {
        var res = new HashSet<(Waypoint wp1, Waypoint wp2)>();
        foreach (var wp in wps)
        {
            var ns = wp.TacNeighbors(data);
            foreach (var nWp in ns)
            {
                if (nWp.Id > wp.Id) continue;
                if (wps.Contains(nWp) == false) continue;
                res.Add((wp, nWp));
            }
        }
        return res;
    }
}