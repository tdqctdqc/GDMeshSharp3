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
            fa => ForceAssignments.Remove(fa),
            key);
        TheaterAssignment.CheckExpandMergeNew(regime,
            ForceAssignments.SelectWhereOfType<TheaterAssignment>().ToList(),
            fa => ForceAssignments.Remove(fa),
                fa => ForceAssignments.Add(fa),
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
                var assgn = new FrontAssignment(front, new HashSet<int>());
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
        var occupiedGroups = ForceAssignments
            .SelectMany(fa => fa.GroupIds)
            .Select(g => data.Get<UnitGroup>(g));
        var freeGroups = data.Military.UnitAux.UnitGroupByRegime[regime]
            ?.Except(occupiedGroups)
            ?.ToList();
        if (freeGroups == null || freeGroups.Count == 0) return;
        
        Assigner.Assign<FrontAssignment, UnitGroup>(
            frontAssgns,
            fa => GetFrontDefenseNeed(fa, data, totalLength, coverLengthWeight, totalOpposing, coverOpposingWeight),
            g => g.GetPowerPoints(data),
            freeGroups.ToHashSet(),
            (fa, g) => fa.GroupIds.Add(g.Id),
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
            return data.Planet.GetOffsetTo(relTo, wp.Pos);
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