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
        var theaters = ForceAssignments.OfType<TheaterAssignment>();
        foreach (var ta in theaters)
        {
            ForceAssignments.Remove(ta);
        }

        var newTheaters = theaters.Blob(regime, key.Data);
        ForceAssignments.AddRange(newTheaters);
        TheaterAssignment.PutGroupsInRightTheater(regime, ForceAssignments,
            key);
        TheaterAssignment.CheckFronts(regime, ForceAssignments.OfType<TheaterAssignment>().ToList(),
            key);
        
        foreach (var ta in ForceAssignments.OfType<TheaterAssignment>().ToList())
        {
            ta.SetTargets(key);
            ta.AssignGroups(key);
        }
        
        foreach (var forceAssignment in ForceAssignments)
        {
            forceAssignment.CalculateOrders(orders, key);
        }
    }
    public IEnumerable<FrontAssignment> GetFrontAssignments()
    {
        return ForceAssignments.OfType<FrontAssignment>();
    }


    private static HashSet<(Waypoint wp1, Waypoint wp2)> GetEdgesWithin(HashSet<Waypoint> wps, Vector2 relTo, Data data)
    {
        var res = new HashSet<(Waypoint wp1, Waypoint wp2)>();
        foreach (var wp in wps)
        {
            var ns = wp.GetNeighbors(data);
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