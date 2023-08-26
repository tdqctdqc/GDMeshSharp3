using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


public class SimpleNavGenerator : Generator
{
    private GenWriteKey _key;
    public override GenReport Generate(GenWriteKey key)
    {
        _key = key;
        var report = new GenReport(nameof(SimpleNavGenerator));
        var nav = Nav.Create(key);
        var byPoly = new Dictionary<MapPolygon, Waypoint>();
        var byEdge = new Dictionary<MapPolygonEdge, Waypoint>();
        var id = new IdDispenser();
        
        AddPolyCenterWps(key, id, nav, byPoly);
        
        AddPolyEdgeWps(key, byPoly, id, byEdge, nav);
        
        ConnectPolyEdgeWps(key, byEdge);
        
        ConnectEdgeWpsAroundNexi(key, byEdge, byPoly);
        
        ConnectInterior(key, nav);

        return report;
    }

    private static void ConnectInterior(GenWriteKey key, Nav nav)
    {
        foreach (var poly in key.Data.GetAll<MapPolygon>())
        {
            var assoc = nav.GetPolyAssocWaypoints(poly, key.Data).ToList();

            foreach (var wp1 in assoc)
            {
                foreach (var wp2 in assoc)
                {
                    if (wp1 == wp2) continue;
                    bool connect = true;
                    foreach (var wp3 in assoc)
                    {
                        if (wp1 == wp3 || wp2 == wp3) continue;
                        var axis = key.Data.Planet.GetOffsetTo(wp1.Pos, wp2.Pos);
                        var offset = key.Data.Planet.GetOffsetTo(wp1.Pos, wp3.Pos);
                        var close = Geometry2D.GetClosestPointToSegment(offset, Vector2.Zero, axis);
                        if (offset.DistanceTo(close) < 50f)
                        {
                            connect = false;
                            break;
                        }
                    }

                    if (connect) Connect(wp1, wp2);
                }
            }
        }
    }

    private static void ConnectEdgeWpsAroundNexi(GenWriteKey key, Dictionary<MapPolygonEdge, Waypoint> byEdge, Dictionary<MapPolygon, Waypoint> byPoly)
    {
        foreach (var nexus in key.Data.GetAll<MapPolyNexus>())
        {
            var incidentPolys = nexus.IncidentPolys.Items(key.Data).Distinct();
            if (incidentPolys.Count() == 3)
            {
                var p0 = incidentPolys.ElementAt(0);
                var p1 = incidentPolys.ElementAt(1);
                var p2 = incidentPolys.ElementAt(2);

                IEnumerable<Waypoint> edgeWps = new Waypoint[0];
                if (p0.Neighbors.Contains(p1)
                    && byEdge.TryGetValue(p0.GetEdge(p1, key.Data), out var e01))
                {
                    edgeWps = edgeWps.Union(e01.Yield());
                }

                if (p1.Neighbors.Contains(p2)
                    && byEdge.TryGetValue(p1.GetEdge(p2, key.Data), out var e12))
                {
                    edgeWps = edgeWps.Union(e12.Yield());
                }

                if (p2.Neighbors.Contains(p0)
                    && byEdge.TryGetValue(p2.GetEdge(p0, key.Data), out var e20))
                {
                    edgeWps = edgeWps.Union(e20.Yield());
                }

                foreach (var e1 in edgeWps)
                {
                    foreach (var e2 in edgeWps)
                    {
                        if (e1 == e2) continue;
                        Connect(e1, e2);
                    }
                }
            }
            else if (incidentPolys.Count() == 2)
            {
                var p0 = incidentPolys.ElementAt(0);
                var wp0 = byPoly[p0];
                var p1 = incidentPolys.ElementAt(1);
                var wp1 = byPoly[p1];
                Connect(wp0, wp1);
            }
            else
            {
                GD.Print($"{incidentPolys.Count()} nexus near {incidentPolys.First().Id}");
            }
        }
    }

    private static void ConnectPolyEdgeWps(GenWriteKey key, Dictionary<MapPolygonEdge, Waypoint> byEdge)
    {
        foreach (var poly in key.Data.GetAll<MapPolygon>())
        {
            var neighborChain = poly
                .GetPolyBorders().ToList().Chainify<PolyBorderChain, Vector2>()
                .Select(b => b.Foreign.Entity(key.Data)).ToList();
            for (var i = 0; i < neighborChain.Count; i++)
            {
                var n = neighborChain.Modulo(i);
                var m = neighborChain.Modulo(i + 1);
                if (n.Neighbors.Contains(m))
                {
                    var edgeN = poly.GetEdge(n, key.Data);
                    var edgeM = poly.GetEdge(m, key.Data);
                    if (byEdge.ContainsKey(edgeN) && byEdge.ContainsKey(edgeM))
                    {
                        var edgeWpN = byEdge[edgeN];
                        var edgeWpM = byEdge[edgeM];
                        Connect(edgeWpM, edgeWpN);
                    }
                }
            }
        }
    }

    private static void AddPolyEdgeWps(GenWriteKey key, Dictionary<MapPolygon, Waypoint> byPoly, IdDispenser id, Dictionary<MapPolygonEdge, Waypoint> byEdge, Nav nav)
    {
        foreach (var edge in key.Data.GetAll<MapPolygonEdge>())
        {
            var hi = edge.HighPoly.Entity(key.Data);
            var lo = edge.LowPoly.Entity(key.Data);
            if (hi.IsWater() && lo.IsWater())
            {
                Connect(byPoly[hi], byPoly[lo]);
                continue;
            }

            var p = hi.GetBorder(lo.Id).Segments.GetPointAlong(.5f) + hi.Center;
            var wp = Waypoint.Construct(key, id.GetID(), p,
                hi, lo);
            byEdge.Add(edge, wp);

            if (hi.IsWater() && lo.IsWater())
            {
                wp.SetType(new SeaNav(), key);
            }
            else if (hi.IsWater() || lo.IsWater())
            {
                var water = hi.IsWater() ? hi : lo;
                var sea = key.Data.Planet.PolygonAux.LandSea.SeaDic[water];
                wp.SetType(CoastNav.Construct(sea.Id), key);
            }
            else if (edge.IsRiver())
            {
                wp.SetType(new RiverNav(), key);
            }
            else
            {
                wp.SetType(new InlandNav(), key);
            }

            nav.Waypoints.Add(wp.Id, wp);
            var hiWp = byPoly[hi];
            var loWp = byPoly[lo];
            Connect(wp, hiWp);
            Connect(wp, loWp);

            nav.PolyNavPaths.Add(hi.GetIdEdgeKey(lo), new List<int> { hiWp.Id, wp.Id, loWp.Id });
        }
    }

    private static void Connect(Waypoint wp1, Waypoint wp2)
    {
        wp1.Neighbors.Add(wp2.Id);
        wp2.Neighbors.Add(wp1.Id);
    }

    private static void AddPolyCenterWps(GenWriteKey key, IdDispenser id, Nav nav, Dictionary<MapPolygon, Waypoint> byPoly)
    {
        foreach (var poly in key.Data.GetAll<MapPolygon>())
        {
            var wp = Waypoint.Construct(key, id.GetID(), poly.Center, poly);
            nav.Waypoints.Add(wp.Id, wp);
            nav.MakeCenterPoint(poly, wp, key);
            if (poly.IsWater())
            {
                wp.SetType(new SeaNav(), key);
            }
            else
            {
                wp.SetType(new InlandNav(), key);
            }

            byPoly.Add(poly, wp);
        }
    }
    
    
}