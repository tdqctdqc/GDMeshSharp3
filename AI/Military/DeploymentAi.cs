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
        CreateFronts(regime, key);
        FillFronts(regime, key.Data);
        foreach (var forceAssignment in ForceAssignments)
        {
            forceAssignment.CalculateOrders(orders, key);
        }
    }
    public IEnumerable<FrontAssignment> GetFrontAssignments()
    {
        return ForceAssignments.SelectWhereOfType<FrontAssignment>();
    }
    
    private void CreateFronts(Regime regime, LogicWriteKey key)
    {
        var alliance = regime.GetAlliance(key.Data);
        var allianceAi = key.Data.HostLogicData.AllianceAis[alliance];
        var responsibility = allianceAi.MilitaryAi.AreasOfResponsibility[regime];

        ForceAssignments.Clear();
            
        
        var unions = UnionFind.Find(responsibility, (wp1, wp2) =>
        {
            return responsibility.Contains(wp1) == responsibility.Contains(wp2);
        }, wp => wp.TacNeighbors(key.Data));
        
        foreach (var union in unions)
        {
            var contactLines = GetContactLines(regime, union.ToHashSet(), key.Data);
            foreach (var contactLine in contactLines)
            {
                var front = Front.Construct(regime, contactLine.Select(wp => wp.Id).ToList(),
                    key);
                var assgn = new FrontAssignment(front);
                ForceAssignments.Add(assgn);
            }
        }
    }
    private void FillFronts(Regime regime, Data data)
    {
        var frontAssgns = GetFrontAssignments().ToList();
        var totalLength = frontAssgns.Sum(fa => fa.Front.GetLength(data));
        var totalOpposing = frontAssgns.Sum(fa => fa.Front.GetOpposingPowerPoints(data));
        var coverLengthWeight = 1f;
        var coverOpposingWeight = 1f;
        var occupiedGroups = ForceAssignments.SelectMany(fa => fa.Groups);
        var freeGroups = data.Military.UnitAux.UnitGroupByRegime[regime]
            ?.Except(occupiedGroups)
            ?.ToList();
        if (freeGroups == null || freeGroups.Count == 0) return;
        
        Assigner.Assign<FrontAssignment, UnitGroup>(
            frontAssgns,
            fa => GetFrontDefenseNeed(fa, data, totalLength, coverLengthWeight, totalOpposing, coverOpposingWeight),
            g => g.GetPowerPoints(data),
            freeGroups.ToHashSet(),
            (fa, g) => fa.Groups.Add(g),
            (fa, g) => g.GetPowerPoints(data));
    }
    
    private float GetFrontDefenseNeed(FrontAssignment fa, Data data, 
        float totalLength, float coverLengthWeight,
        float totalOpposing, float coverOpposingWeight)
    {
        var opposing = fa.Front.GetOpposingPowerPoints(data);
        var length = fa.Front.GetLength(data);

        var res = 0f;
        if (totalOpposing != 0f)
        {
            res += coverOpposingWeight * opposing / totalOpposing;
        }

        if (totalLength != 0f)
        {
            res += coverLengthWeight * length / totalLength;
        }
        return res;
    }
    public static List<List<Waypoint>> GetContactLines(Regime regime, HashSet<Waypoint> wps, 
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
                .Where(isHostile);
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
                var include = true;
                foreach (var e2 in threatenedEdges)
                {
                    if (e1 == e2) continue;
                    if (
                        arcsOverlap(e1, e2, hostile)
                        && 
                        blockedBy(e1, e2, hostile)
                        )
                    {
                        include = false;
                        break;
                    }
                }

                if (include)
                {
                    var max = Mathf.Max(e1.wp1.Id, e1.wp2.Id);
                    var min = Mathf.Min(e1.wp1.Id, e1.wp2.Id);
                    frontEdges.Add(new Vector2I(min, max));
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
        
        return chains
            .Select(c => c.GetPoints())
            .Select(ps => ps.Select(p => data.Military.TacticalWaypoints.ByPos[p])
                .Select(i => data.Military.TacticalWaypoints.Waypoints[i])
                .ToList())
            .ToList();

        bool arcsOverlap((Waypoint, Waypoint) edge, 
            (Waypoint, Waypoint) blocker, 
            Waypoint objective)
        {
            var o = relPos(objective);
            var e1 = relPos(edge.Item1) - o;
            var e2 = Mathf.Abs(e1.AngleTo(relPos(edge.Item2) - o));
            var b1 = Mathf.Abs(e1.AngleTo(relPos(blocker.Item1) - o));
            var b2 = Mathf.Abs(e1.AngleTo(relPos(blocker.Item2) - o));
            return b1 < e2 || b2 < e2;
        }
        bool blockedBy((Waypoint, Waypoint) edge, 
            (Waypoint, Waypoint) blocker, 
            Waypoint objective)
        {
            var o = relPos(objective);
            var e1 = relPos(edge.Item1);
            var e2 = relPos(edge.Item2);
            var ls1 = new LineSegment(e1, Vector2.Zero);
            var ls2 = new LineSegment(e2, Vector2.Zero);
            var b1 = relPos(blocker.Item1);
            var b2 = relPos(blocker.Item2);

            return ls1.IntersectsExclusive(b1, b2) || ls2.IntersectsExclusive(b1, b2);
        }
        
        Vector2 relPos(Waypoint wp)
        {
            return data.Planet.GetOffsetTo(relTo, wp.Pos);
        }
        bool isThreatened(Waypoint wp)
        {
            return wp.IsDirectlyThreatened(alliance, data)
                || (wp is IRiverWaypoint && wp.IsIndirectlyThreatened(alliance, data))
                || wp.TacNeighbors(data)
                        .Any(n => n is IRiverWaypoint && n.IsIndirectlyThreatened(alliance, data));
        }
        
        bool isHostile(Waypoint wp)
        {
            if (wps.Contains(wp) == false) return false;
            return wp.IsDirectlyThreatened(alliance, data)
                   || (wp is IRiverWaypoint && wp.IsIndirectlyThreatened(alliance, data));
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