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

    public List<List<Waypoint>> GetLinesByFindingClockwise(Data d)
    {
        var alliance = Regime.Entity(d).GetAlliance(d);
        var wps = GetTacWaypoints(d);
        var potentialEdges = wps.SelectMany(wp =>
        {
            return wp.Neighbors.Where(nId => nId < wp.Id)
                .Where(nId => TacWaypointIds.Contains(nId))
                .Select(nId => new Vector2I(wp.Id, nId));
        }).ToHashSet();
        var edges = new HashSet<Vector2I>();
        
        foreach (var pEdge in potentialEdges)
        {
            var edgeFrom = MilitaryDomain.GetTacWaypoint(pEdge.X, d);
            var edgeTo = MilitaryDomain.GetTacWaypoint(pEdge.Y, d);
            var fromHostiles = edgeFrom.TacNeighbors(d)
                .Where(n => TacWaypointIds.Contains(n.Id) == false
                            && n.IsDirectlyThreatened(alliance, d)
                            && intersectsWithPotential(edgeFrom, n) == false).ToList();
            if (fromHostiles.Count == 0 && edgeFrom.IsDirectlyThreatened(alliance, d) == false) continue;
            var toHostiles = edgeTo.TacNeighbors(d)
                .Where(n => TacWaypointIds.Contains(n.Id) == false
                            && n.IsDirectlyThreatened(alliance, d)
                            && intersectsWithPotential(edgeTo, n) == false).ToList();
            if (toHostiles.Count == 0 && edgeTo.IsDirectlyThreatened(alliance, d) == false) continue;
            var hostiles = fromHostiles.Union(toHostiles);
            var axis = edgeFrom.Pos.GetOffsetTo(edgeTo.Pos, d);
            var reverseAxis = -(edgeFrom.Pos.GetOffsetTo(edgeTo.Pos, d));
            var hasInHem1 = hostiles.Any(h =>
            {
                var angle = axis.GetCCWAngleTo(edgeFrom.Pos.GetOffsetTo(h.Pos, d));
                return angle <= Mathf.Pi && angle >= 0;
            });
            var hasInHem2 = hostiles.Any(h =>
            {
                var angle = axis.GetCCWAngleTo(edgeFrom.Pos.GetOffsetTo(h.Pos, d));
                return angle > Mathf.Pi && angle < Mathf.Pi * 2f;
            });
            var fromShared = 
                getAllEdgesSharingPoint(edgeFrom.Id).ToList();
            if (fromShared.Count() == 0)
            {
                edges.Add(pEdge);
                continue;
            }
            var toShared = 
                getAllEdgesSharingPoint(edgeTo.Id).ToList();
            if (toShared.Count() == 0)
            {
                edges.Add(pEdge);
                continue;
            }
            if (hasInHem1)
            {
                var fromRay = getNextCcw(edgeFrom, edgeTo, fromShared);
                var toRay = getNextCw(edgeTo, edgeFrom, toShared);
                if (doesntMakeTriOrCross(edgeFrom, edgeTo,
                        fromRay, toRay))
                {
                    edges.Add(pEdge);
                    continue;
                }
            }
            if (hasInHem2)
            {
                var fromRay = getNextCw(edgeFrom, edgeTo, fromShared);
                var toRay = getNextCcw(edgeTo, edgeFrom, toShared);
                if (doesntMakeTriOrCross(edgeTo, edgeFrom,
                        toRay, fromRay))
                {
                    edges.Add(pEdge);
                }
            }

            Vector2I getNextCw(Waypoint from, Waypoint to, IEnumerable<Vector2I> fromSharedPointEdges)
            {
                return fromSharedPointEdges.MinBy(e =>
                    {
                        int other;
                        if (e.X == from.Id) other = e.Y;
                        else other = e.X;
                        var otherPos = MilitaryDomain.GetTacWaypoint(other, d).Pos;
                        return from.Pos.GetOffsetTo(to.Pos, d)
                            .GetCWAngleTo(from.Pos.GetOffsetTo(otherPos, d));
                    });
            }
            Vector2I getNextCcw(Waypoint from, Waypoint to, IEnumerable<Vector2I> fromSharedPointEdges)
            {
                return fromSharedPointEdges
                    .MinBy(e =>
                    {
                        int other;
                        if (e.X == from.Id) other = e.Y;
                        else other = e.X;
                        var otherPos = MilitaryDomain.GetTacWaypoint(other, d).Pos;
                        return from.Pos.GetOffsetTo(to.Pos, d)
                            .GetCCWAngleTo(from.Pos.GetOffsetTo(otherPos, d));
                    });
            }

            bool intersectsWithPotential(Waypoint wp1, Waypoint wp2)
            {
                return potentialEdges.Any(edge =>
                {
                    var e1 = MilitaryDomain.GetTacWaypoint(edge.X, d).Pos;
                    var e2 = e1.GetOffsetTo(MilitaryDomain.GetTacWaypoint(edge.Y, d).Pos, d);
                    var p1 = e1.GetOffsetTo(wp1.Pos, d);
                    var p2 = e1.GetOffsetTo(wp2.Pos, d);
                    return Vector2Ext.LineSegIntersect(Vector2.Zero, e2, p1, p2, false, out _);
                });
            }
            bool doesntMakeTriOrCross(Waypoint from, Waypoint to,
                Vector2I fromRay, Vector2I toRay)
            {
                var fromRayId = fromRay.X == from.Id ? fromRay.Y : fromRay.X;
                var fromRayWp = MilitaryDomain.GetTacWaypoint(fromRayId, d);
                
                // var fromAngle = from.Pos.GetOffsetTo(to.Pos, d)
                //     .GetCCWAngleTo(from.Pos.GetOffsetTo(fromRayWp.Pos, d));
                // if (fromAngle > Mathf.Pi && fromAngle < Mathf.Pi * 2f)
                // {
                //     return true;
                // }
                
                var toRayId = toRay.X == to.Id ? toRay.Y : toRay.X;
                var toRayWp = MilitaryDomain.GetTacWaypoint(toRayId, d);
                
                // var toAngle = to.Pos.GetOffsetTo(from.Pos, d)
                //     .GetCCWAngleTo(to.Pos.GetOffsetTo(toRayWp.Pos, d));
                // if (toAngle > Mathf.Pi && toAngle < Mathf.Pi * 2f)
                // {
                //     return true;
                // }

                if (fromRayId == toRayId) return false;
                if(Vector2Ext.LineSegIntersect(Vector2.Zero,
                       from.Pos.GetOffsetTo(fromRayWp.Pos, d),
                       from.Pos.GetOffsetTo(to.Pos, d),
                       from.Pos.GetOffsetTo(toRayWp.Pos, d),
                       true, out _))
                {
                    return false;
                }

                return true;
            }
            IEnumerable<Vector2I> getAllEdgesSharingPoint(int id)
            {
                return potentialEdges
                    .Where(p => p != pEdge 
                        && (p.X == id || p.Y == id));
            }
        }

        return edges.Select(getLineSeg).ToList()
            .GetChains()
            .Select(c => c.GetPoints().Select(p => d.Military.TacticalWaypoints.ByPos[p])
                .Select(id => MilitaryDomain.GetTacWaypoint(id, d)).ToList())
            .ToList();

        LineSegment getLineSeg(Vector2I edge)
        {
            return new LineSegment(MilitaryDomain.GetTacWaypoint(edge.X, d).Pos,
                MilitaryDomain.GetTacWaypoint(edge.Y, d).Pos);
        }
    }

    public List<List<Waypoint>> GetLinesByTris(Data d)
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
        return edges.Select(getLineSeg).ToList()
            .GetChains()
            .Select(c => c.GetPoints().Select(p => d.Military.TacticalWaypoints.ByPos[p])
                .Select(id => MilitaryDomain.GetTacWaypoint(id, d)).ToList())
            .ToList();

        
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

            
            //todo restrict hostiles to certain cone
            
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
        var lines = GetLinesByTris(d)
            .ToDictionary(l => l, 
                l => new List<FrontSegmentAssignment>());
        var segments = Assignments.WhereOfType<FrontSegmentAssignment>().ToList();
        foreach (var fsa in segments)
        {
            var relatedLines = lines.Keys
                .Where(l => l.Any(wp => fsa.LineWaypointIds.Contains(wp.Id)));
            if (relatedLines.Count() == 0)
            {
                Assignments.Remove(fsa);
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
                Assignments.Add(seg);
                continue;
            }

            var biggest = segs.MaxBy(s => s.GroupIds.Count());
            
            biggest.LineWaypointIds.Clear();
            biggest.LineWaypointIds.AddRange(line.Select(wp => wp.Id));
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