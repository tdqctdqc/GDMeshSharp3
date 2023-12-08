using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;


public class NavGenerator : Generator
{
    private GenWriteKey _key;
    private static float _angleThreshold = GeometryExt.DegreesToRad(5f);
    
    public override GenReport Generate(GenWriteKey key)
    {
        _key = key;
        var report = new GenReport(nameof(NavGenerator));
        var nav = NavWaypoints.Create(key);
        var byPoly = new Dictionary<MapPolygon, Waypoint>();
        var byEdge = new Dictionary<MapPolygonEdge, Waypoint>();
        var id = key.Data.IdDispenser;
        
        AddPolyCenterWps(key, id, nav, byPoly);
        
        AddPolyEdgeWps(key, byPoly, id, byEdge, nav);
        
        AddRiverMouthWps(key, id, byPoly, byEdge, nav);
        
        ConnectPolyEdgeWps(key, byEdge);
        
        ConnectEdgeWpsAroundNexi(key, byEdge, byPoly);

        ConnectEdgeWpsToLateralPolyCenters(byEdge, byPoly);
        
        ConnectInterior(key, nav);
        
        RemoveBadConnections();
        
        SetLandWaypointProperties();

        key.Data.Notices.MadeWaypoints.Invoke();
        return report;
    }

    private void ConnectEdgeWpsToLateralPolyCenters(Dictionary<MapPolygonEdge, Waypoint> byEdge,
        Dictionary<MapPolygon, Waypoint> byPoly)
    {
        foreach (var kvp in byEdge)
        {
            var edge = kvp.Key;
            var wp = kvp.Value;
            var hiNexus = edge.HiNexus.Entity(_key.Data);
            var loNexus = edge.LoNexus.Entity(_key.Data);
            var laterals = hiNexus.IncidentPolys.Items(_key.Data)
                .Union(loNexus.IncidentPolys.Items(_key.Data))
                .Where(p => edge.EdgeToPoly(p) == false);
            
            foreach (var lateral in laterals)
            {
                if (lateral.IsWater()) continue;
                MapPolyNexus nexus;
                var nexi = lateral.GetNexi(_key.Data);
                
                if (hiNexus.IncidentPolys.Items(_key.Data).Contains(lateral))
                {
                    nexus = hiNexus;
                }
                else if (loNexus.IncidentPolys.Items(_key.Data).Contains(lateral))
                {
                    nexus = loNexus;
                }
                else throw new Exception();
                var lateralWp = byPoly[lateral];

                if (nexus.IncidentEdges.Items(_key.Data)
                    .Any(e => e.IsRiver() 
                              && e.LineCrosses(lateralWp.Pos, wp.Pos, _key.Data)))
                {
                    continue;
                }
                
                var relevantWps = 
                    edge.HighPoly.Entity(_key.Data).GetAssocNavWaypoints(_key.Data)
                    .Union(edge.LowPoly.Entity(_key.Data).GetAssocNavWaypoints(_key.Data))
                    .Union(lateral.GetAssocNavWaypoints(_key.Data))
                    .Distinct();
                if (CloseByDist(wp, lateralWp, 10f, relevantWps, _key))
                {
                    continue;
                }
                Connect(wp, lateralWp);
            }
        }
    }

    private static void AddRiverMouthWps(GenWriteKey key, IdDispenser id, Dictionary<MapPolygon, Waypoint> byPoly, Dictionary<MapPolygonEdge, Waypoint> byEdge, NavWaypoints navWaypoints)
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
            var wp = new RiverMouthWaypoint(key, id.TakeId(), nexus.Point,
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
                    if (eWp is ICoastWaypoint == false && eWp is RiverWaypoint == false)
                    {
                        GD.Print("strange river mouth wp at poly " + seaPoly.Id);
                    }

                    Connect(wp, eWp);
                }
            }

            navWaypoints.AddWaypoint(wp, key);
        }
    }

    private static void ConnectInterior(GenWriteKey key, NavWaypoints navWaypoints)
    {
        foreach (var poly in key.Data.GetAll<MapPolygon>())
        {
            if (poly.IsWater()) continue;
            var assoc = navWaypoints.GetPolyAssocWaypoints(poly, key.Data).ToList();

            foreach (var wp1 in assoc)
            {
                foreach (var wp2 in assoc)
                {
                    if (wp1 == wp2) continue;
                    if (CloseByAngle(wp1, wp2, assoc, key)) continue;
                    Connect(wp1, wp2);
                }
            }
        }
    }
    private static bool CloseByDist(Waypoint wp1, Waypoint wp2, float dist, IEnumerable<Waypoint> wps,
        GenWriteKey key)
    {
        foreach (var wp3 in wps)
        {
            if (wp1 == wp3 || wp2 == wp3) continue;
            var pos1 = wp3.Pos.GetOffsetTo(wp1.Pos, key.Data);
            var pos2 = wp3.Pos.GetOffsetTo(wp2.Pos, key.Data);
            if (Vector2.Zero.DistToLine(pos1, pos2) < dist) return true;
        }

        return false;
    }
    private static bool CloseByAngle(Waypoint wp1, Waypoint wp2, IEnumerable<Waypoint> wps,
         GenWriteKey key)
    {
        foreach (var wp3 in wps)
        {
            if (wp1 == wp3 || wp2 == wp3) continue;
            var pos1 = wp3.Pos.GetOffsetTo(wp1.Pos, key.Data);
            var pos2 = wp3.Pos.GetOffsetTo(wp2.Pos, key.Data);

            var angle1 = -pos1.AngleTo(pos2 - pos1);
            var angle2 = -pos2.AngleTo(pos1 - pos2);
            if (angle1 < _angleThreshold || angle2 < _angleThreshold) return true;
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
                    bool close = CloseByAngle(wp1, wp2, toConnect, key);
                    if (close) continue;
                    
                    if (close == false) Connect(wp1, wp2);
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

    private static void AddPolyEdgeWps(GenWriteKey key, Dictionary<MapPolygon, Waypoint> byPoly, IdDispenser id, Dictionary<MapPolygonEdge, Waypoint> byEdge, NavWaypoints navWaypoints)
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
                wp = new SeaWaypoint(key, id.TakeId(), p, hi, lo);
            }
            else if (hi.IsWater() || lo.IsWater())
            {
                var water = hi.IsWater() ? hi : lo;
                var sea = key.Data.Planet.PolygonAux.LandSea.SeaDic[water];
                wp = new CoastWaypoint(key, sea.Id, false, id.TakeId(), p, hi, lo);
            }
            else if (edge.IsRiver())
            {
                wp = new RiverWaypoint(key, id.TakeId(), p, hi, lo);
            }
            else
            {
                wp = new InlandWaypoint(key, id.TakeId(), p, hi, lo);
            }
            byEdge.Add(edge, wp);

            navWaypoints.AddWaypoint(wp, key);
            var hiWp = byPoly[hi];
            var loWp = byPoly[lo];
            Connect(wp, hiWp);
            Connect(wp, loWp);

            navWaypoints.PolyNavPaths.Add(hi.GetIdEdgeKey(lo), new List<int> { hiWp.Id, wp.Id, loWp.Id });
        }
    }

    private static void Connect(Waypoint wp1, Waypoint wp2)
    {
        wp1.Neighbors.Add(wp2.Id);
        wp2.Neighbors.Add(wp1.Id);
    }
    private static void Disconnect(Waypoint wp1, Waypoint wp2)
    {
        wp1.Neighbors.Remove(wp2.Id);
        wp2.Neighbors.Remove(wp1.Id);
    }
    private static void AddPolyCenterWps(GenWriteKey key, IdDispenser id, NavWaypoints navWaypoints, Dictionary<MapPolygon, Waypoint> byPoly)
    {
        foreach (var poly in key.Data.GetAll<MapPolygon>())
        {
            Waypoint wp;
            if (poly.IsWater())
            {
                wp = new SeaWaypoint(key, id.TakeId(), poly.Center, poly);
            }
            else
            {
                wp = new InlandWaypoint(key, id.TakeId(), poly.Center, poly);
            }
            navWaypoints.AddWaypoint(wp, key);
            navWaypoints.MakeCenterPoint(poly, wp, key);
            byPoly.Add(poly, wp);
        }
    }

    private void RemoveBadConnections()
    {
        var nav = _key.Data.Planet.NavWaypoints;
        foreach (var wp in nav.Waypoints.Values)
        {
            if (wp is CoastWaypoint == false) continue;
            var assocPolys = wp.AssocPolys(_key.Data).Where(p => p.IsWater());
            if (assocPolys.Count() == 0) continue;
            foreach (var p in assocPolys)
            {
                var wpPos = p.GetOffsetTo(wp.Pos, _key.Data);
                foreach (var nId in wp.Neighbors.ToList())
                {
                    var nWp = nav.Waypoints[nId];
                    if (nWp is SeaWaypoint) continue;
                    var nWpPos = p.GetOffsetTo(nWp.Pos, _key.Data);
                    if (p.LineEntersPoly(wpPos, nWpPos, _key.Data))
                    {
                        Disconnect(wp, nWp);
                    }
                }
            }
        }
    }
    private void SetLandWaypointProperties()
    {
        var nav = _key.Data.Planet.NavWaypoints;
        var waypoints = nav.Waypoints.Values;
        foreach (var waypoint in waypoints)
        {
            if (waypoint is ILandWaypoint n == false) continue;
            var pos = waypoint.Pos.ClampPosition(_key.Data);

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
                var totalArea = roughSample.Sum(s => s.GetArea());
                var totalRoughness = roughSample.Sum(s => s.Landform(_key.Data).MinRoughness * s.GetArea());



                var roughVal = totalRoughness / totalArea;
                
                if (float.IsNaN(roughVal) == false)
                {
                    rTotal += roughVal;
                }
            }
            
            n.SetRoughness(rTotal / numAssoc, _key);
        }
    }
}