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
    public HashSet<int> TacWaypointIds { get; private set; }

    public HashSet<ForceAssignment> Assignments { get; private set; }
    public FrontAssignment(
        int id,
        EntityRef<Regime> regime, 
        HashSet<int> tacWaypointIds,
        HashSet<int> groupIds,
        HashSet<ForceAssignment> assignments) 
        : base(groupIds, regime, id)
    {
        TacWaypointIds = tacWaypointIds;
        Assignments = assignments;
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
        var length = Assignments.WhereOfType<FrontSegmentAssignment>().Sum(s => s.GetLength(data));
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
            {
                if (theater.TacWaypointIds.Contains(i) == false) return true;
                var wp = MilitaryDomain.GetTacWaypoint(i, key.Data);
                if (wp.IsThreatened(alliance, key.Data) == false) return true;
                if (alliance.Members.Contains(wp.GetOccupyingRegime(key.Data)) == false) return true;
                return false;
            });
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
                    new HashSet<int>(), new HashSet<ForceAssignment>()));
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
        var wps = GetTacWaypoints(d);
        var potentialEdges = wps
            .SelectMany(wp =>
            {
                return wp.Neighbors.Where(nId => nId < wp.Id)
                    .Where(nId => TacWaypointIds.Contains(nId))
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
                .Select(id => MilitaryDomain.GetTacWaypoint(id, d)).ToList())
            .ToList();;
        
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
            var wpA = MilitaryDomain.GetTacWaypoint(a, d);
            var wpB = MilitaryDomain.GetTacWaypoint(b, d);
            var wpC = MilitaryDomain.GetTacWaypoint(c, d);
            
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
            return wp.TacNeighbors(d)
                .Where(n => TacWaypointIds.Contains(n.Id));
        }
        
        IEnumerable<Waypoint> getHostiles(Waypoint wp)
        {
            return wp.TacNeighbors(d)
                .Where(n => TacWaypointIds.Contains(n.Id) == false
                            && n.IsDirectlyThreatened(alliance, d));
        }


        bool getCw(Vector2 a, Vector2 c)
        {
            return a.GetCWAngleTo(c) < Mathf.Pi;
        }
        
        
        LineSegment getLineSeg(Vector2I edge)
        {
            return new LineSegment(MilitaryDomain.GetTacWaypoint(edge.X, d).Pos,
                MilitaryDomain.GetTacWaypoint(edge.Y, d).Pos);
        }

        bool hasNonIntersectingHostileRay(Vector2I edge)
        {
            var wp1 = MilitaryDomain.GetTacWaypoint(edge.X, d);
            var wp2 = MilitaryDomain.GetTacWaypoint(edge.Y, d);
            var wp1Hostiles = getHostiles(wp1);
            var wp2Hostiles = getHostiles(wp2);
            return wp1Hostiles.Any(h => intersectsWithPotential(new Vector2I(edge.X, h.Id)) == false)
                   && wp2Hostiles.Any(h => intersectsWithPotential(new Vector2I(edge.Y, h.Id)) == false);
        }
        bool intersectsWithPotential(Vector2I edge)
        {
            var wp1 = MilitaryDomain.GetTacWaypoint(edge.X, d);
            var wp2 = MilitaryDomain.GetTacWaypoint(edge.Y, d);
            return potentialEdges.Any(pEdge =>
            {
                var e1 = MilitaryDomain.GetTacWaypoint(pEdge.X, d).Pos;
                var e2 = e1.GetOffsetTo(MilitaryDomain.GetTacWaypoint(pEdge.Y, d).Pos, d);
                var p1 = e1.GetOffsetTo(wp1.Pos, d);
                var p2 = e1.GetOffsetTo(wp2.Pos, d);
                return Vector2Ext.LineSegIntersect(Vector2.Zero, e2, p1, p2, false, out _);
            });
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
        var frontline = ta.GetTacWaypoints(key.Data)
            .Where(wp => wp.IsThreatened(alliance, key.Data)
                && wp.GetOccupyingRegime(key.Data) == r);
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
                    new HashSet<int>(), new HashSet<ForceAssignment>());
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
        Assignments.AddRange(merging.Assignments);
    }


    public void CheckSegments(LogicWriteKey key)
    {
        var d = key.Data;
        var lines = GetLines(d)
            .ToDictionary(l => l, 
                l => new List<FrontSegmentAssignment>());
        
        var segments = Assignments.WhereOfType<FrontSegmentAssignment>().ToList();
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
                    .Construct(Regime, line, null, false, key);
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
}