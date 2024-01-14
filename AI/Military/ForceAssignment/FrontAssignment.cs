using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using GDMeshSharp3.Exception;
using Godot;
using MathNet.Numerics.Statistics;

public class FrontAssignment : ForceAssignment, ICompoundForceAssignment
{
    
    public static float CoverLengthWeight = 1f;
    public static float CoverOpposingWeight = 3f;
    public static float PowerPointsPerLengthToCover = .1f;
    public HashSet<int> HeldWaypointIds { get; private set; }
    public HashSet<int> TargetAreaWaypointIds { get; private set; }
    public HashSet<ForceAssignment> Assignments { get; private set; }
    public FrontAssignment(
        int id,
        EntityRef<Regime> regime, 
        HashSet<int> heldWaypointIds,
        HashSet<int> targetAreaWaypointIds,
        HashSet<int> groupIds,
        HashSet<ForceAssignment> assignments) 
        : base(groupIds, regime, id)
    {
        HeldWaypointIds = heldWaypointIds;
        TargetAreaWaypointIds = targetAreaWaypointIds;
        Assignments = assignments;
    }

    public IEnumerable<Waypoint> HeldWaypoints(Data data)
    {
        return HeldWaypointIds.Select(i => MilitaryDomain.GetWaypoint(i, data));
    }

    public float GetOpposingPowerPoints(Data data)
    {
        var forceBalances = data.Context.WaypointForceBalances;
        var alliance = Regime.Entity(data).GetAlliance(data);
        return HeldWaypoints(data)
            .Sum(wp => forceBalances[wp].GetHostilePowerPoints(alliance, data));
    }
    public override void CalculateOrders(MinorTurnOrders orders, 
        LogicWriteKey key)
    {
        foreach (var assgn in Assignments)
        {
            assgn.CalculateOrders(orders, key);
        }
    }
    

    public override void TakeAwayGroup(UnitGroup g, LogicWriteKey key)
    {
        this.TakeAwayGroupCompound(g, key);
    }

    public override void AssignGroups(LogicWriteKey key)
    {
        
        this.ShiftGroups(key);
        this.AssignFreeGroups(key);
        foreach (var assgn in Assignments)
        {
            assgn.AssignGroups(key);
        }
    }
    
    
    public override float GetPowerPointNeed(Data data)
    {
        var opposing = GetOpposingPowerPoints(data);
        var length = Assignments.OfType<FrontSegmentAssignment>().Sum(s => s.GetLength(data));
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
            fa.HeldWaypointIds.RemoveWhere(i =>
            {
                if (theater.TacWaypointIds.Contains(i) == false) return true;
                var wp = MilitaryDomain.GetWaypoint(i, key.Data);
                if (wp.IsThreatened(alliance, key.Data) == false) return true;
                if (wp.IsControlled(alliance, key.Data) == false) return true;
                return false;
            });
            if (fa.HeldWaypointIds.Count == 0)
            {
                remove(fa);
                continue;
            }
            var flood = FloodFill<Waypoint>.GetFloodFill(
                fa.HeldWaypoints(key.Data).First(),
                wp => fa.HeldWaypointIds.Contains(wp.Id),
                wp => wp.GetNeighbors(key.Data));
            var unions = UnionFind.Find(fa.HeldWaypointIds,
                (i, j) => true,
                i => MilitaryDomain.GetWaypoint(i, key.Data).Neighbors);

            if (unions.Count() != 1)
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
        var unions = UnionFind.Find(fa.HeldWaypointIds,
            (i, j) => true,
            i => MilitaryDomain.GetWaypoint(i, key.Data).Neighbors);

        var newFronts =
            unions.Select(
                u => new FrontAssignment(key.Data.IdDispenser.TakeId(), r.MakeRef(), u.ToHashSet(),
                    new HashSet<int>(),
                    new HashSet<int>(), 
                    new HashSet<ForceAssignment>()));
        foreach (var group in fa.Groups(key.Data))
        {
            var unitWpIds = group.Units.Items(key.Data)
                .Select(u => key.Data.Context.UnitWaypoints[u].Id).ToList();
            var mostWpsShared = newFronts
                .MaxBy(t => unitWpIds.Where(t.HeldWaypointIds.Contains).Count());
            if (unitWpIds.Where(mostWpsShared.HeldWaypointIds.Contains).Count() == 0)
            {
                var pos = group.GetPosition(key.Data);
                var closest = newFronts
                    .MinBy(t => 
                        group.GetPosition(key.Data).GetOffsetTo(
                            key.Data.Planet.GetAveragePosition(t.HeldWaypointIds.Select(i =>
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

        return newFronts;
    }
    public List<List<Waypoint>> GetLines(Data d)
    {
        var alliance = Regime.Entity(d).GetAlliance(d);
        var wps = HeldWaypoints(d);
        var potentialEdges = wps
            .SelectMany(wp =>
            {
                return wp.Neighbors.Where(nId => nId < wp.Id)
                    .Where(nId => HeldWaypointIds.Contains(nId))
                    .Select(nId => new Vector2I(wp.Id, nId));
            })
            .ToHashSet();
        var edges = potentialEdges.ToHashSet();
        foreach (var tri in getTris())
        {
            checkEdge(tri.X, tri.Y, tri.Z);
            checkEdge(tri.X, tri.Z,  tri.Y);
            checkEdge(tri.Y, tri.Z, tri.X);
        }
        edges = edges.Where(hasNonIntersectingHostileRay).ToHashSet();
        return OrderSegments(edges, d);
        
        
        HashSet<Vector3I> getTris()
        {
            var res = new HashSet<Vector3I>();
            foreach (var wp in wps)
            {
                var neighborsInside = getNeighborsInside(wp);
                foreach (var n1 in neighborsInside)
                {
                    if (n1.Id > wp.Id) continue;
                    var mutualNeighbors = neighborsInside
                        .Intersect(getNeighborsInside(n1));
                    foreach (var n2 in mutualNeighbors)
                    {
                        if (n2.Id > n1.Id) continue;
                        res.Add(new Vector3I(wp.Id, n1.Id, n2.Id));
                    }
                }
            }

            return res;
        }
        void checkEdge(int a, int b, int c)
        {
            var wpA = MilitaryDomain.GetWaypoint(a, d);
            var wpB = MilitaryDomain.GetWaypoint(b, d);
            var wpC = MilitaryDomain.GetWaypoint(c, d);
            
            var axis = wpA.Pos.GetOffsetTo(wpB.Pos, d);
            var rAxis = -axis;
            var ray = wpA.Pos.GetOffsetTo(wpC.Pos, d);
            bool cw = getCw(axis, ray);
            var validHostiles = getHostiles(wpA)
                .Union(getHostiles(wpB))
                .Where(h =>
                {
                    var hostileRayA = wpA.Pos.GetOffsetTo(h.Pos, d);
                    var hostileRayB = wpB.Pos.GetOffsetTo(h.Pos, d);
                    return getCw(axis, hostileRayA) != cw
                           && axis.AngleTo(hostileRayA) < Mathf.Pi / 2f
                           && rAxis.AngleTo(hostileRayB) < Mathf.Pi / 2f;
                });

            if (validHostiles.Count() == 0)
            {
                edges.Remove(new Vector2I(a, b));
            }
        }
        IEnumerable<Waypoint> getNeighborsInside(Waypoint wp)
        {
            return wp.GetNeighbors(d)
                .Where(n => HeldWaypointIds.Contains(n.Id));
        }
        IEnumerable<Waypoint> getHostiles(Waypoint wp)
        {
            return wp.GetNeighbors(d)
                .Where(n => HeldWaypointIds.Contains(n.Id) == false
                            && n.IsDirectlyThreatened(alliance, d));
        }
        bool getCw(Vector2 a, Vector2 c)
        {
            return a.GetCWAngleTo(c) < Mathf.Pi;
        }
        bool hasNonIntersectingHostileRay(Vector2I edge)
        {
            var wp1 = MilitaryDomain.GetWaypoint(edge.X, d);
            var wp2 = MilitaryDomain.GetWaypoint(edge.Y, d);
            var wp1Hostiles = getHostiles(wp1);
            var wp2Hostiles = getHostiles(wp2);
            return wp1Hostiles.Any(h => intersectsWithPotential(new Vector2I(edge.X, h.Id)) == false)
                   && wp2Hostiles.Any(h => intersectsWithPotential(new Vector2I(edge.Y, h.Id)) == false);
        }
        bool intersectsWithPotential(Vector2I edge)
        {
            var wp1 = MilitaryDomain.GetWaypoint(edge.X, d);
            var wp2 = MilitaryDomain.GetWaypoint(edge.Y, d);
            return potentialEdges.Any(pEdge =>
            {
                var e1 = MilitaryDomain.GetWaypoint(pEdge.X, d).Pos;
                var e2 = e1.GetOffsetTo(MilitaryDomain.GetWaypoint(pEdge.Y, d).Pos, d);
                var p1 = e1.GetOffsetTo(wp1.Pos, d);
                var p2 = e1.GetOffsetTo(wp2.Pos, d);
                return Vector2Ext.LineSegIntersect(Vector2.Zero, e2, p1, p2, false, out _);
            });
        }
    }

    private List<List<Waypoint>> OrderSegments(
        HashSet<Vector2I> edges, Data d)
    {
        var chains = edges
            .Select(getLineSeg).ToList()
            .GetChains();
        var res = new List<List<Vector2>>();
        for (var i = 0; i < chains.Count; i++)
        {
            var chain = chains[i];
            if (chain.Count <=
                FrontSegmentAssignment.IdealSegmentLength * 1.5f)
            {
                res.Add(chain.GetPoints());
                continue;
            }
            var numChains = Mathf.FloorToInt((float)chain.Count / FrontSegmentAssignment.IdealSegmentLength);
            var numLinksPerChain = Mathf.CeilToInt((float)chain.Count / numChains);
            var iter = 0;
            var curr = new List<LineSegment>();
            for (var j = 0; j < chain.Count; j++)
            {
                curr.Add(chain[j]);
                iter++;
                if (iter == numLinksPerChain || j == chain.Count - 1)
                {
                    res.Add(curr.GetPoints());
                    iter = 0;
                    curr = new List<LineSegment>();
                }
            }
        }

        return res
            .Select(c => c.Select(p => d.Military.TacticalWaypoints.ByPos[p])
                .Select(id => MilitaryDomain.GetWaypoint(id, d)).ToList())
            .ToList();
        
        LineSegment getLineSeg(Vector2I edge)
        {
            return new LineSegment(MilitaryDomain.GetWaypoint(edge.X, d).Pos,
                MilitaryDomain.GetWaypoint(edge.Y, d).Pos);
        }
    }
    public static void CheckExpandMergeNew(Regime r, 
        TheaterAssignment ta,
        List<FrontAssignment> fronts, 
        Action<FrontAssignment> add,
        Action<FrontAssignment> remove,
        LogicWriteKey key)
    {
        var alliance = r.GetAlliance(key.Data);
        var frontline = 
            ta.GetWaypoints(key.Data)
            .Where(wp => wp.IsThreatened(alliance, key.Data)
                && wp.GetOccupyingRegime(key.Data) == r);
        var unions = UnionFind.Find(frontline, (w, v) => true,
            w => w.GetNeighbors(key.Data));
        var dic = unions.ToDictionary(u => u, u => new List<FrontAssignment>());
        foreach (var fa in fronts)
        {
            var faWps = fa.HeldWaypoints(key.Data);
            var mostShared = unions
                .MaxBy(u => u.Where(wp => fa.HeldWaypointIds.Contains(wp.Id)).Count());

            if (mostShared.Where(wp => fa.HeldWaypointIds.Contains(wp.Id)).Count() == 0)
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
                    new HashSet<int>(),
                    new HashSet<int>(),
                    new HashSet<ForceAssignment>());
                add(fa);
                continue;
            }
            var biggest = kvp.Value.MaxBy(fa => fa.HeldWaypointIds.Count());
            biggest.HeldWaypointIds.Clear();
            biggest.HeldWaypointIds.AddRange(kvp.Key.Select(wp => wp.Id));
            foreach (var fa in kvp.Value)
            {
                if (fa == biggest) continue;
                biggest.MergeInto(fa, key);
                remove(fa);
            }
        }

    }

    public void MergeInto(FrontAssignment dissolve, LogicWriteKey key)
    {
        GroupIds.AddRange(dissolve.GroupIds);
        Assignments.AddRange(dissolve.Assignments);
    }


    public void CheckSegments(LogicWriteKey key)
    {
        var d = key.Data;
        var lines = GetLines(d)
            .ToDictionary(l => l, 
                l => new List<FrontSegmentAssignment>());
        
        var segments = Assignments.OfType<FrontSegmentAssignment>().ToList();
        foreach (var fsa in segments)
        {
            var relatedLines = lines.Keys
                .Where(l => l.Any(wp => fsa.FrontLineWpIds.Contains(wp.Id)));
            if (relatedLines.Count() == 0)
            {
                Assignments.Remove(fsa);
                continue; 
            }

            var mostRelated = relatedLines
                .MaxBy(l => l.Where(wp => fsa.FrontLineWpIds.Contains(wp.Id)).Count());
            lines[mostRelated].Add(fsa);
        }
        
        foreach (var kvp in lines)
        {
            var line = kvp.Key;
            var segs = kvp.Value;
            if (segs.Count() == 0)
            {
                var seg = FrontSegmentAssignment
                    .Construct(Regime, line, 
                        null, false, key);
                Assignments.Add(seg);
                continue;
            }

            var biggest = segs.MaxBy(s => s.GroupIds.Count());
            
            biggest.FrontLineWpIds.Clear();
            biggest.FrontLineWpIds.AddRange(line.Select(wp => wp.Id));
            foreach (var fsa in segs)
            {
                if (fsa == biggest) continue;
                biggest.MergeInto(fsa, key);
                Assignments.Remove(fsa);
            }
        }
    }

    public override Waypoint GetCharacteristicWaypoint(Data d)
    {
        return HeldWaypointIds.Select(i => MilitaryDomain.GetWaypoint(i, d))
            .FirstOrDefault(wp => wp.GetOccupyingRegime(d).Id == Regime.RefId);
    }

    public override UnitGroup RequestGroup(LogicWriteKey key)
    {
        if (GroupIds.Count < 2) return null;
        UnitGroup deassign = null;
        if (Assignments.Count > 0)
        {
            deassign = Assignments
                .MaxBy(s => s.GetSatisfiedRatio(key.Data))
                .RequestGroup(key);
        }
        
        if(deassign == null)
        {
            deassign = key.Data.Get<UnitGroup>(GroupIds.First());
        }
        
        if(deassign != null) GroupIds.Remove(deassign.Id);
        return deassign;
    }

    public void SetTargets(LogicWriteKey key)
    {
        var alliance = Regime.Entity(key.Data).GetAlliance(key.Data);
        var wps = HeldWaypoints(key.Data);
        var targets = expand(wps);
        // targets = expand(targets);
        TargetAreaWaypointIds.Clear();
        TargetAreaWaypointIds.AddRange(targets.Select(t => t.Id));
        IEnumerable<Waypoint> expand(IEnumerable<Waypoint> source)
        {
            return source
                .SelectMany(wp => wp.GetNeighbors(key.Data)
                    .Where(n => n.IsHostile(alliance, key.Data)));
        }
        foreach (var seg in Assignments.OfType<FrontSegmentAssignment>())
        {
            seg.SetAdvance(true, key);
        }
    }

    public IEnumerable<Waypoint> GetWaypoints(Data d)
    {
        return HeldWaypointIds.Select(id => MilitaryDomain.GetWaypoint(id, d));
    }
}