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
        // CreateFronts(regime, key);
        // FillFronts(regime, key.Data);
        
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
        TheaterAssignment.CheckTheaterFronts(regime, ForceAssignments.SelectWhereOfType<TheaterAssignment>().ToList(),
            key);
        
        foreach (var forceAssignment in ForceAssignments)
        {
            forceAssignment.CalculateOrders(orders, key);
        }
    }
    public IEnumerable<FrontAssignment> GetFrontAssignments()
    {
        return ForceAssignments.SelectWhereOfType<FrontAssignment>();
    }

    
    
    
    
    public static List<List<Waypoint>> GetContactLines(Regime regime, 
        HashSet<Waypoint> wps, 
        Data data)
    {
        var context = data.Context;
        var fbs = context.WaypointForceBalances;
        var alliance = regime.GetAlliance(data);
        var relTo = wps.First().Pos;

        //key is hostile, value is wps incident
        var byThreat = new Dictionary<Waypoint, HashSet<Waypoint>>();
        var threatenedWps = new HashSet<Waypoint>();
        foreach (var wp in wps)
        {
            var ns = wp.TacNeighbors(data)
                .Where(n => n.IsDirectlyThreatened(alliance, data))
                .Where(n => n.IsControlled(alliance, data) == false);
                
            if (ns.Count() == 0) continue;
            threatenedWps.Add(wp);
            foreach (var n in ns)
            {
                byThreat
                    .GetOrAdd(n, x => new HashSet<Waypoint>())
                    .Add(wp);
            }
        }
        if (threatenedWps.Count == 1)
            return new List<List<Waypoint>> { new List<Waypoint> { threatenedWps.First() } };
        
        var frontEdges = new HashSet<Vector2I>();
        foreach (var kvp in byThreat)
        {
            var hostile = kvp.Key;
            var threatenedEdges = GetEdgesWithin(kvp.Value, relTo, data);
            foreach (var e1 in threatenedEdges)
            {
                var max = Mathf.Max(e1.wp1.Id, e1.wp2.Id);
                var min = Mathf.Min(e1.wp1.Id, e1.wp2.Id);
                var id = new Vector2I(min, max);
                if (frontEdges.Contains(id)) continue;

                var include = true;
                foreach (var e2 in threatenedEdges)
                {
                    if (e1 == e2) continue;
                    if (blockedBy(e1, e2, hostile))
                    {
                        include = false;
                        break;
                    }
                }

                if (include)
                {
                    frontEdges.Add(id);
                }
            }
        }
        
        var chains = LineSegmentExt.GetChains(
            frontEdges.Select(v => 
                    new LineSegment(
                        data.Military.TacticalWaypoints.Waypoints[v.X].Pos,
                        data.Military.TacticalWaypoints.Waypoints[v.Y].Pos
                    ))
                .ToList());

        var pointLists = chains
            .Select(c => c.GetPoints());

        var wpLists = pointLists.Select(ps => ps.Select(p => data.Military.TacticalWaypoints.ByPos[p])
            .Select(i => data.Military.TacticalWaypoints.Waypoints[i])
            .ToList()).ToList();
        return wpLists;

        bool blockedBy((Waypoint, Waypoint) edge, 
            (Waypoint, Waypoint) blocker, 
            Waypoint objective)
        {
            var o = relPos(objective);
            var e1 = relPos(edge.Item1);
            var e2 = relPos(edge.Item2);
            var b1 = relPos(blocker.Item1);
            var b2 = relPos(blocker.Item2);

            return Vector2Ext.LineSegIntersect(e1, o, b1, b2, false, out _)
                   || Vector2Ext.LineSegIntersect(e2, o, b1, b2, false, out _)
                   || Vector2Ext.LineSegIntersect((e1 + e2) / 2f, o, b1, b2, false, out _)
                   || Vector2Ext.LineSegIntersect(e1, e2, b1, b2, false, out _)
                   ;
        }
        
        Vector2 relPos(Waypoint wp)
        {
            return relTo.GetOffsetTo(wp.Pos, data);
        }
        
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