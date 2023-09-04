using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


public class NavGenerator : Generator
{
    private GenWriteKey _key;
    public override GenReport Generate(GenWriteKey key)
    {
        _key = key;
        var report = new GenReport(nameof(NavGenerator));
        var nav = Nav.Create(key);
        var byPoly = new Dictionary<MapPolygon, Waypoint>();
        var byEdge = new Dictionary<MapPolygonEdge, Waypoint>();
        var id = new IdDispenser();
        
        AddPolyCenterWps(key, id, nav, byPoly);
        
        AddPolyEdgeWps(key, byPoly, id, byEdge, nav);
        
        
        AddRiverMouthWps(key, id, byPoly, byEdge, nav);
        
        ConnectPolyEdgeWps(key, byEdge);
        
        ConnectEdgeWpsAroundNexi(key, byEdge, byPoly);
        
        ConnectInterior(key, nav);
        
        SetLandWaypointProperties();

        return report;
    }

    private static void AddRiverMouthWps(GenWriteKey key, IdDispenser id, Dictionary<MapPolygon, Waypoint> byPoly, Dictionary<MapPolygonEdge, Waypoint> byEdge, Nav nav)
    {
        foreach (var nexus in key.Data.GetAll<MapPolyNexus>())
        {
            if (nexus.IsRiverNexus(key.Data) == false) continue;
            var incidentPolys = nexus.IncidentPolys.Items(key.Data);
            if (incidentPolys.FirstOrDefault(
                    p => p.IsWater()) is MapPolygon seaPoly == false)
            {
                continue;
            }

            var sea = key.Data.Planet.PolygonAux.LandSea.SeaDic[seaPoly];
            var count = incidentPolys.Count();
            var wp = new RiverMouthWaypoint(key, id.GetID(), nexus.Point,
                sea.Id,
                count > 0 ? incidentPolys.ElementAt(0) : null,
                count > 1 ? incidentPolys.ElementAt(1) : null,
                count > 2 ? incidentPolys.ElementAt(2) : null,
                count > 3 ? incidentPolys.ElementAt(3) : null
            );
            foreach (var incidentPoly in incidentPolys)
            {
                var pWp = byPoly[incidentPoly];
                Connect(wp, pWp);
            }

            var incidentEdges = nexus.IncidentEdges.Items(key.Data);
            foreach (var incidentEdge in incidentEdges)
            {
                if (byEdge.TryGetValue(incidentEdge, out var eWp))
                {
                    if (eWp is RiverWaypoint == false && eWp is CoastWaypoint == false)
                    {
                        // throw new Exception();   
                        GD.Print("strange river mouth wp at poly " + seaPoly.Id);
                    }

                    Connect(wp, eWp);
                }
            }

            nav.Waypoints.Add(wp.Id, PolymorphMember<Waypoint>.Construct(wp));
        }
    }

    private static void ConnectInterior(GenWriteKey key, Nav nav)
    {
        foreach (var poly in key.Data.GetAll<MapPolygon>())
        {
            if (poly.IsWater()) continue;
            var assoc = nav.GetPolyAssocWaypoints(poly, key.Data).ToList();

            foreach (var wp1 in assoc)
            {
                foreach (var wp2 in assoc)
                {
                    if (wp1 == wp2) continue;
                    bool close = Close(50f, wp1, wp2, assoc, key);
                    if (close == false) Connect(wp1, wp2);
                }
            }
        }
    }

    private static bool Close(float dist, Waypoint wp1, Waypoint wp2, IEnumerable<Waypoint> wps,
         GenWriteKey key)
    {
        foreach (var wp3 in wps)
        {
            if (wp1 == wp3 || wp2 == wp3) continue;
            var axis = key.Data.Planet.GetOffsetTo(wp1.Pos, wp2.Pos);
            var offset = key.Data.Planet.GetOffsetTo(wp1.Pos, wp3.Pos);
            var close = Geometry2D.GetClosestPointToSegment(offset, Vector2.Zero, axis);
            if (offset.DistanceTo(close) < 50f)
            {
                return true;
            }
        }

        return false;
    }

    private static void ConnectEdgeWpsAroundNexi(GenWriteKey key, 
        Dictionary<MapPolygonEdge, Waypoint> byEdge, Dictionary<MapPolygon, Waypoint> byPoly)
    {
        foreach (var nexus in key.Data.GetAll<MapPolyNexus>())
        {
            var incidentPolys = nexus.IncidentPolys.Items(key.Data).Distinct();
            if (incidentPolys.Any(p => p.IsWater())) continue;
            if (incidentPolys.Count() >= 3)
            {
                connectAll(incidentPolys.ToArray());
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
                GD.Print($"{incidentPolys.Count()} nexus ");
                foreach (var incidentPoly in incidentPolys)
                {
                    GD.Print($"\t at {incidentPoly.Id}");
                }
                throw new Exception();
            }
        }

        void connectAll(params MapPolygon[] polys)
        {
            HashSet<Waypoint> toConnect = new HashSet<Waypoint>();
            foreach (var p1 in polys)
            {
                foreach (var p2 in polys)
                {
                    if (p1 == p2) continue;
                    if (p1.Id < p2.Id) continue;
                    if (p1.Neighbors.Contains(p2)
                        && byEdge.TryGetValue(p1.GetEdge(p2, key.Data), out var e12))
                    {
                        toConnect.Add(e12);
                    }
                }
            }
            foreach (var wp1 in toConnect)
            {
                foreach (var wp2 in toConnect)
                {
                    if (wp1 == wp2) continue;
                    bool close = Close(50f, wp1, wp2, toConnect, key);
                    if (close == false) Connect(wp1, wp2);
                    
                    //todo check here for coast waypoints if the connection is crossing water to 
                    //another land wp
                }
            }
        }
    }

    private static void ConnectPolyEdgeWps(GenWriteKey key, Dictionary<MapPolygonEdge, Waypoint> byEdge)
    {
        foreach (var poly in key.Data.GetAll<MapPolygon>())
        {
            if (poly.IsWater()) continue;
            var borders = poly
                .GetPolyBorders().ToList();
            borders.OrderByClockwise(Vector2.Zero, c => c.Segments.First().From);
            var neighborChain = borders
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
            
            Waypoint wp;
            if (hi.IsWater() && lo.IsWater())
            {
                wp = new SeaWaypoint(key, id.GetID(), p, hi, lo);
            }
            else if (hi.IsWater() || lo.IsWater())
            {
                var water = hi.IsWater() ? hi : lo;
                var sea = key.Data.Planet.PolygonAux.LandSea.SeaDic[water];
                wp = new CoastWaypoint(key, sea.Id, false, id.GetID(), p, hi, lo);
            }
            else if (edge.IsRiver())
            {
                wp = new RiverWaypoint(key, id.GetID(), p, hi, lo);
            }
            else
            {
                wp = new InlandWaypoint(key, id.GetID(), p, hi, lo);
            }
            byEdge.Add(edge, wp);

            nav.Waypoints.Add(wp.Id, PolymorphMember<Waypoint>.Construct(wp));
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
            Waypoint wp;
            if (poly.IsWater())
            {
                wp = new SeaWaypoint(key, id.GetID(), poly.Center, poly);
            }
            else
            {
                wp = new InlandWaypoint(key, id.GetID(), poly.Center, poly);
            }
            nav.Waypoints.Add(wp.Id, PolymorphMember<Waypoint>.Construct(wp));
            nav.MakeCenterPoint(poly, wp, key);
            byPoly.Add(poly, wp);
        }
    }
    
    private void SetLandWaypointProperties()
    {
        var nav = _key.Data.Planet.Nav;
        var waypoints = nav.Waypoints.Values.Select(p => p.Value());
        foreach (var waypoint in waypoints)
        {
            if (waypoint is ILandWaypoint n == false) continue;
            var pos = _key.Data.Planet.ClampPosition(waypoint.Pos);

            var rTotal = 0f;
            var assoc = waypoint.AssocPolys(_key.Data);
            var numAssoc = assoc.Count();
            foreach (var poly in assoc)
            {
                var offset = poly.GetOffsetTo(waypoint.Pos, _key.Data);
                var roughSample= poly.Tris.Tris
                    .Where(t => t.Landform(_key.Data).IsLand)
                    .OrderBy(t => t.GetCentroid().DistanceTo(offset))
                    .Take(4);
                if (roughSample.Count() == 0) continue;
                var roughVal = roughSample.Average(t => t.Landform(_key.Data).MinRoughness);
                
                if (float.IsNaN(roughVal) == false)
                {
                    rTotal += roughVal;
                }
            }
            
            n.SetRoughness(rTotal / numAssoc, _key);
        }
    }
}