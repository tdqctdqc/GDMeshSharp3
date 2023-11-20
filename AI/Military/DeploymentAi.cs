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
        }, wp => wp.GetNeighboringTacWaypoints(key.Data));
        
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
        var alliance = regime.GetAlliance(data);
        var relTo = wps.First().Pos;

        bool isIndirectlyThreatened(Waypoint wp, HashSet<Waypoint> covered)
        {
            var ns = wp.GetNeighboringTacWaypoints(data);
            var threatenedNeighbor = ns.Any(
                n => n.IsDirectlyThreatened(alliance, data) 
                && covered.Contains(n) == false
            );
            if (threatenedNeighbor) return true;
            
            var threatenedOverRiver = ns.Any(
                n => n is IRiverWaypoint
                    && n.IsIndirectlyThreatened(alliance, data)
            );
            return threatenedOverRiver;
        }

        var directlyThreatened = wps
            .Where(wp => wp.IsDirectlyThreatened(alliance, data))
            .ToHashSet();
        
        var indirectlyThreatened = wps
            .Where(wp => isIndirectlyThreatened(wp, wps))
            .ToHashSet();
        
        var threatenedWps =
            directlyThreatened
                .Union(indirectlyThreatened)
                .Distinct()
                .ToHashSet();
        
        if (threatenedWps.Count == 1)
            return new List<List<Waypoint>> { new List<Waypoint> { threatenedWps.First() } };
        
        
        var threatenedEdges = GetEdgesWithin(threatenedWps, relTo, data);
        var threatenedEdgeLineSegs = threatenedEdges
            .Select(wps =>
            {
                return new LineSegment(data.Planet.GetOffsetTo(relTo, wps.wp1.Pos),
                    data.Planet.GetOffsetTo(relTo, wps.wp2.Pos));
            }).ToList();

        bool isHostile(Waypoint n)
        {
            return n.IsDirectlyThreatened(alliance, data)
                   || (n is IRiverWaypoint && n.IsIndirectlyThreatened(alliance, data));
        }

        bool intersectsThreatenedEdge(Waypoint from, Waypoint to)
        {
            var o1 = data.Planet.GetOffsetTo(relTo, from.Pos);
            var o2 = data.Planet.GetOffsetTo(relTo, to.Pos);
            return threatenedEdgeLineSegs.Any(ls => 
                ls.IntersectsExclusive(o1, o2));
        }

        bool intersectsThreatenedEdgeV2(Vector2 from, Vector2 to)
        {
            return threatenedEdgeLineSegs.Any(ls => 
                ls.IntersectsExclusive(from, to));
        }

        var hostileWps = threatenedWps
            .SelectMany(wp => wp.GetNeighboringTacWaypoints(data)
                .Where(isHostile))
            .ToHashSet();



        var closeToHostile = hostileWps
            .Select(h => threatenedWps
                .MinBy(wp => data.Planet.GetOffsetTo(h.Pos, wp.Pos).Length()))
            .ToHashSet();
        

        var goodEdges = 
            GetEdgesWithin(closeToHostile, relTo, data)
            .Select(wps => new LineSegment(wps.wp1.Pos,
                wps.wp2.Pos)); 

        var chains = LineSegmentExt.GetChains(goodEdges.ToList());
        
        return chains
            .Select(c => c.GetPoints())
            .Select(ps => ps.Select(p => data.Military.TacticalWaypoints.ByPos[p])
                .Select(i => data.Military.TacticalWaypoints.Waypoints[i])
                .ToList())
            .ToList();
    }

    private static HashSet<(Waypoint wp1, Waypoint wp2)> GetEdgesWithin(HashSet<Waypoint> wps, Vector2 relTo, Data data)
    {
        var res = new HashSet<(Waypoint wp1, Waypoint wp2)>();
        foreach (var wp in wps)
        {
            var ns = wp.GetNeighboringTacWaypoints(data);
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