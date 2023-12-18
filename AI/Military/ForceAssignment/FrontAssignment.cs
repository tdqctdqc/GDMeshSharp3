using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using GDMeshSharp3.Exception;
using Godot;
using MathNet.Numerics.Statistics;

public class FrontAssignment : ForceAssignment
{
    
    public static float CoverLengthWeight = 1f;
    public static float CoverOpposingWeight = 3f;
    public static float PowerPointsPerLengthToCover = .1f;
    public HashSet<int> TacWaypointIds { get; private set; }

    public List<FrontSegmentAssignment> Segments { get; private set; }

    public FrontAssignment(
        int id,
        EntityRef<Regime> regime, 
        HashSet<int> tacWaypointIds,
        HashSet<int> groupIds,
        List<FrontSegmentAssignment> segments) 
        : base(groupIds, regime, id)
    {
        TacWaypointIds = tacWaypointIds;
        Segments = segments;
    }
    public Vector2 RelTo(Data d)
    {
        var p = MilitaryDomain.GetTacWaypoint(TacWaypointIds.First(), d).Pos;
        return p.ClampPosition(d);
    }

    public IEnumerable<Waypoint> GetTacWaypoints(Data data)
    {
        return TacWaypointIds.Select(i => MilitaryDomain.GetTacWaypoint(i, data));
    }

    public float GetOpposingPowerPoints(Data data)
    {
        var forceBalances = data.Context.WaypointForceBalances;
        var alliance = Regime.Entity(data).GetAlliance(data);
        return GetTacWaypoints(data)
            .Sum(wp => forceBalances[wp].GetHostilePowerPoints(alliance, data));
    }
    public override void CalculateOrders(MinorTurnOrders orders, 
        LogicWriteKey key)
    {
        foreach (var fsa in Segments)
        {
            fsa.CalculateOrders(orders, key);
        }
    }


    public void AssignGroups(LogicWriteKey key)
    {
        ShiftGroups(key);
        AssignFreeGroups(key);
    }

    private void ShiftGroups(LogicWriteKey key)
    {
        if (Segments.Count < 2) return;
        var data = key.Data;

        var max = maxSatisfied();
        var min = minSatisfied();
        var iter = 0;
        while (iter < Segments.Count * 2 
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

        (float ratio, FrontSegmentAssignment fa) maxSatisfied()
        {
            var max = Segments.MaxBy(fa => fa.GetSatisfiedRatio(data));
            return (max.GetSatisfiedRatio(data), max);
        }

        (float ratio, FrontSegmentAssignment fa) minSatisfied()
        {
            var min = Segments.MinBy(fa => fa.GetSatisfiedRatio(data));
            return (min.GetSatisfiedRatio(data), min);
        }
    }
    private void AssignFreeGroups(LogicWriteKey key)
    {
        var alliance = Regime.Entity(key.Data).GetAlliance(key.Data);
        var totalLength = TacWaypointIds.Count;
        var totalOpposing = GetOpposingPowerPoints(key.Data);
        var coverLengthWeight = 1f;
        var coverOpposingWeight = 1f;
        var occupiedGroups = Segments
            .SelectMany(fa => fa.GroupIds)
            .Select(g => key.Data.Get<UnitGroup>(g));
        var freeGroups = Groups(key.Data)
            ?.Except(occupiedGroups)
            ?.ToList();
        if (freeGroups == null || freeGroups.Count == 0) return;
        
        Assigner.Assign<FrontSegmentAssignment, UnitGroup>(
            Segments,
            fa => fa.GetPowerPointNeed(key.Data),
            fsa => fsa.Groups(key.Data),
            g => g.GetPowerPoints(key.Data),
            freeGroups.ToHashSet(),
            (fa, g) => fa.GroupIds.Add(g.Id),
            (fa, g) => g.GetPowerPoints(key.Data));

    }
    
    
    public override float GetPowerPointNeed(Data data)
    {
        var opposing = GetOpposingPowerPoints(data);
        var length = Segments.Sum(s => s.GetLength(data));
        return opposing * CoverOpposingWeight + length * CoverLengthWeight * PowerPointsPerLengthToCover;
    }
    public static void CheckSplitRemove(Regime r, 
        TheaterAssignment theater,
        List<FrontAssignment> fronts, 
        Action<FrontAssignment> add, Action<FrontAssignment> remove, LogicWriteKey key)
    {
        var alliance = r.GetAlliance(key.Data);
        for (var i = 0; i < fronts.Count; i++)
        {
            var fa = fronts[i];
            fa.TacWaypointIds.RemoveWhere(i => 
                theater.TacWaypointIds.Contains(i) == false
                || MilitaryDomain.GetTacWaypoint(i, key.Data).IsThreatened(alliance, key.Data) == false);
            if (fa.TacWaypointIds.Count == 0)
            {
                remove(fa);
                continue;
            }
            var flood = FloodFill<Waypoint>.GetFloodFill(fa.GetTacWaypoints(key.Data).First(),
                wp => fa.TacWaypointIds.Contains(wp.Id),
                wp => wp.TacNeighbors(key.Data));
            if (flood.Count() != fa.TacWaypointIds.Count)
            {
                remove(fa);
                var newFronts = Divide(fa, key);
                foreach (var newFa in newFronts)
                {
                    add(newFa);
                }
            }
        }
    }
    
    public static IEnumerable<FrontAssignment> Divide(FrontAssignment fa, 
        LogicWriteKey key)
    {
        var r = fa.Regime.Entity(key.Data);
        var unions = UnionFind.Find(fa.TacWaypointIds,
            (i, j) => true,
            i => MilitaryDomain.GetTacWaypoint(i, key.Data).Neighbors);

        var newFronts =
            unions.Select(
                u => new FrontAssignment(key.Data.IdDispenser.TakeId(), r.MakeRef(), u.ToHashSet(),
                    new HashSet<int>(), new List<FrontSegmentAssignment>()));
        foreach (var group in fa.Groups(key.Data))
        {
            var unitWpIds = group.Units.Items(key.Data)
                .Select(u => key.Data.Context.UnitWaypoints[u].Id).ToList();
            var mostWpsShared = newFronts
                .MaxBy(t => unitWpIds.Where(t.TacWaypointIds.Contains).Count());
            if (unitWpIds.Where(mostWpsShared.TacWaypointIds.Contains).Count() == 0)
            {
                var pos = group.GetPosition(key.Data);
                var closest = newFronts
                    .MinBy(t => 
                        group.GetPosition(key.Data).GetOffsetTo(
                            key.Data.Planet.GetAveragePosition(t.TacWaypointIds.Select(i =>
                                MilitaryDomain.GetTacWaypoint(i, key.Data).Pos)),
                            key.Data).Length()
                    );
                closest.GroupIds.Add(group.Id);
            }
            else
            {
                mostWpsShared.GroupIds.Add(group.Id);
            }
        }

        return newFronts;
    }
    public List<List<Waypoint>> GetLines(Data d)
    {
        var alliance = Regime.Entity(d).GetAlliance(d);
        var waypoints = GetTacWaypoints(d)
            .ToHashSet();
        var dic = new Dictionary<Waypoint, HashSet<Waypoint>>();
        
        var segKeys = new HashSet<Vector2I>();

        foreach (var wp in waypoints)
        {
            foreach (var nWp in wp.TacNeighbors(d).Where(waypoints.Contains))
            {
                if (nWp.Id > wp.Id) continue;
                var wpThreats = wp.TacNeighbors(d)
                    .Except(waypoints)
                    .Where(n => n.IsDirectlyThreatened(alliance, d));
                var nWpThreats = nWp.TacNeighbors(d)
                    .Except(waypoints)
                    .Where(n => n.IsDirectlyThreatened(alliance, d));
                var threatsIntersect = wpThreats.Intersect(nWpThreats);
                
                if (threatsIntersect.Count() > 0)
                {
                    foreach (var intersect in threatsIntersect)
                    {
                        dic.GetOrAdd(intersect, i => new HashSet<Waypoint>())
                            .Add(wp);
                        dic.GetOrAdd(intersect, i => new HashSet<Waypoint>())
                            .Add(nWp);
                    }
                }
            }
        }
        foreach (var kvp in dic)
        {
            var hostile = kvp.Key;
            var cands = kvp.Value;
            var passed = cands.Where(pass);
            
            foreach (var wp in passed)
            {
                foreach (var nWp in wp.TacNeighbors(d).Where(passed.Contains))
                {
                    if (nWp.Id > wp.Id) continue;
                    segKeys.Add(new Vector2I(wp.Id, nWp.Id));
                }
            }
            bool pass(Waypoint wp)
            {
                var dist = hostile.Pos.GetOffsetTo(wp.Pos, d).Length();
                return cands.Where(c => c != wp
                                        && wp.Neighbors.Contains(c.Id)
                                        && hostile.Pos.GetOffsetTo(c.Pos, d).Length() < dist * .95f).Count() < 2;
            }
        }

        try
        {
            var chains = segKeys.Select(k => 
                    new LineSegment(
                        MilitaryDomain.GetTacWaypoint(k.X, d).Pos, 
                        MilitaryDomain.GetTacWaypoint(k.Y, d).Pos))
                .ToList()
                .GetChains();
            var res = new List<List<Vector2>>();
            while (chains.Count > 0)
            {
                var chain = chains.Last();
                chains.Remove(chain);
                while (true)
                {
                    var list = chains
                        .FirstOrDefault(c => neighboringLists(chain, c));
                    if (list == null) break;
                    chains.Remove(list);
                    chain = link(chain, list);
                }

                var first = getFirst(chain);
                var second = getWp(chain.First().To);
                var last = getLast(chain);
                var penultimate = getWp(chain.Last().From);
                var t1 = linkEnd(first, second);
                if (t1 != null)
                {
                    chain.Insert(0, new LineSegment(t1.Pos, first.Pos));
                }
                var t2 = linkEnd(last, penultimate);
                if (t2 != null)
                {
                    chain.Add(new LineSegment(last.Pos, t2.Pos));
                }

                if (chain.Count > FrontSegmentAssignment.IdealSegmentLength * 1.5f)
                {
                    var numChains = Mathf.FloorToInt((float)chain.Count / FrontSegmentAssignment.IdealSegmentLength);
                    var numLinksPerChain = Mathf.CeilToInt((float)chain.Count / numChains);
                    var iter = 0;
                    var curr = new List<LineSegment>();
                    for (var i = 0; i < chain.Count; i++)
                    {
                        curr.Add(chain[i]);
                        iter++;
                        if (iter == numLinksPerChain || i == chain.Count - 1)
                        {
                            res.Add(curr.GetPoints());
                            iter = 0;
                            curr = new List<LineSegment>();
                        }
                    }
                }
                else
                {
                    res.Add(chain.GetPoints());
                }
                

                
                Waypoint linkEnd(Waypoint end, Waypoint before)
                {
                    var axis = end.Pos.GetOffsetTo(before.Pos, d);
                    var threats = end.TacNeighbors(d)
                        .Where(n =>
                            TacWaypointIds.Contains(n.Id)
                            && n.IsThreatened(alliance, d)
                            && axis.AngleTo(end.Pos.GetOffsetTo(n.Pos, d)) > Mathf.Pi / 2f);
                    if (threats.Count() > 0)
                    {
                        return threats.First();
                    }

                    return null;
                }
            }
            
            return res.Select(r => r.Select(getWp).ToList()).ToList();
            
            Waypoint getFirst(List<LineSegment> segs)
            {
                return getWp(segs.First().From);
            }
            Waypoint getLast(List<LineSegment> segs)
            {
                return getWp(segs.Last().To);
            }

            Waypoint getWp(Vector2 pos)
            {
                var id = d.Military.TacticalWaypoints.ByPos[pos];
                return MilitaryDomain.GetTacWaypoint(id, d);
            }

            List<LineSegment> link(List<LineSegment> a, List<LineSegment> b)
            {
                var aFirst = getFirst(a);
                var aLast = getLast(a);
                var bFirst = getFirst(b);
                var bLast = getLast(b);
                var res = new List<LineSegment>();
                if (aFirst.Neighbors(bLast))
                {
                    res.AddRange(b);
                    res.AddRange(a);
                }
                else if (aFirst.Neighbors(bFirst))
                {
                    res.AddRange(b.ReverseSegments());
                    res.AddRange(a);
                }
                else if (aLast.Neighbors(bFirst))
                {
                    res.AddRange(a);
                    res.AddRange(b);
                }
                else if (aLast.Neighbors(bLast))
                {
                    res.AddRange(a);
                    res.AddRange(b.ReverseSegments());
                }
                else throw new Exception();

                return res;
            }
            bool neighboringLists(List<LineSegment> g, List<LineSegment> n)
            {
                var gFirst = getFirst(g);
                var gLast = getLast(g);
                var nFirst = getFirst(n);
                var nLast = getLast(n);
                return gFirst.Neighbors.Contains(nFirst.Id)
                       || gLast.Neighbors.Contains(nFirst.Id)
                       || gFirst.Neighbors.Contains(nLast.Id)
                       || gLast.Neighbors.Contains(nLast.Id);
            }
        }
        catch (Exception e)
        {
            GD.Print("failure");
            return new List<List<Waypoint>>();
        }
        return new List<List<Waypoint>>();
    }
    public static void CheckExpandMergeNew(Regime r, 
        TheaterAssignment ta,
        List<FrontAssignment> fronts, 
        Action<FrontAssignment> add,
        Action<FrontAssignment> remove,
        LogicWriteKey key)
    {
        var alliance = r.GetAlliance(key.Data);
        var frontline = ta.GetTacWaypoints(key.Data)
            .Where(wp => wp.IsThreatened(alliance, key.Data));
        var unions = UnionFind.Find(frontline, (w, v) => true,
            w => w.TacNeighbors(key.Data));
        var dic = unions.ToDictionary(u => u, u => new List<FrontAssignment>());
        foreach (var fa in fronts)
        {
            var faWps = fa.GetTacWaypoints(key.Data);
            var mostShared = unions
                .MaxBy(u => u.Where(wp => fa.TacWaypointIds.Contains(wp.Id)).Count());

            if (mostShared.Where(wp => fa.TacWaypointIds.Contains(wp.Id)).Count() == 0)
            {
                remove(fa);
            }
            else
            {
                dic[mostShared].Add(fa);
            }
        }
        foreach (var kvp in dic)
        {
            if (kvp.Value.Count() == 0)
            {
                var fa = new FrontAssignment(key.Data.IdDispenser.TakeId(), r.MakeRef(), kvp.Key.Select(wp => wp.Id).ToHashSet(),
                    new HashSet<int>(), new List<FrontSegmentAssignment>());
                add(fa);
                continue;
            }
            var biggest = kvp.Value.MaxBy(fa => fa.TacWaypointIds.Count());
            biggest.TacWaypointIds.Clear();
            biggest.TacWaypointIds.AddRange(kvp.Key.Select(wp => wp.Id));
            foreach (var fa in kvp.Value)
            {
                if (fa == biggest) continue;
                biggest.MergeInto(fa, key);
                remove(fa);
            }
        }

    }

    public void MergeInto(FrontAssignment merging, LogicWriteKey key)
    {
        GroupIds.AddRange(merging.GroupIds);
        Segments.AddRange(merging.Segments);
    }


    public void CheckSegments(LogicWriteKey key)
    {
        var d = key.Data;
        var lines = GetLines(d)
            .ToDictionary(l => l, 
                l => new List<FrontSegmentAssignment>());
        var segments = Segments.ToList();
        foreach (var fsa in segments)
        {
            var relatedLines = lines.Keys
                .Where(l => l.Any(wp => fsa.LineWaypointIds.Contains(wp.Id)));
            if (relatedLines.Count() == 0)
            {
                Segments.Remove(fsa);
                continue; 
            }

            var mostRelated = relatedLines
                .MaxBy(l => l.Where(wp => fsa.LineWaypointIds.Contains(wp.Id)).Count());
            lines[mostRelated].Add(fsa);
        }
        
        foreach (var kvp in lines)
        {
            var line = kvp.Key;
            var segs = kvp.Value;
            if (segs.Count() == 0)
            {
                var seg = FrontSegmentAssignment
                    .Construct(Regime, line, key);
                Segments.Add(seg);
                continue;
            }

            var biggest = segs.MaxBy(s => s.GroupIds.Count());
            
            biggest.LineWaypointIds.Clear();
            biggest.LineWaypointIds.AddRange(line.Select(wp => wp.Id));
            foreach (var fsa in segs)
            {
                if (fsa == biggest) continue;
                biggest.MergeInto(fsa, key);
                Segments.Remove(fsa);
            }
        }
    }

    public UnitGroup DeassignGroup(LogicWriteKey key)
    {
        if (GroupIds.Count == 0) return null;
        UnitGroup deassign = null;
        if (Segments.Count > 0)
        {
            deassign = Segments
                .MaxBy(s => s.GetSatisfiedRatio(key.Data))
                .DeassignGroup(key);
        }
        
        if(deassign == null)
        {
            deassign = key.Data.Get<UnitGroup>(GroupIds.First());
        }
        
        if(deassign != null) GroupIds.Remove(deassign.Id);
        return deassign;
    }
}