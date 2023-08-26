using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class NavGenerator : Generator
{
    private GenWriteKey _key;
    private IdDispenser _id;
    private Bijection<MapPolygon, Waypoint> _centerPoints;
    private Bijection<MapPolygonEdge, Waypoint> _edgePoints;
    private Bijection<MapPolyNexus, Waypoint> _nexusPoints;
    private Bijection<Vector2, Waypoint> _interiorPoints;
    private float _nonRiverEdgeSplitLength = 50f;
    public override GenReport Generate(GenWriteKey key)
    {
        _key = key;
        var report = new GenReport(nameof(NavGenerator));
        _id = new IdDispenser();
        var nav = Nav.Create(key);

        MakeCenterNavPoints(key.Data);
        MakeNexusNavPoints(key.Data);
        MakeEdgePoints(key.Data);
        MakeInteriorPoints(key.Data);
        LinkPoints(key.Data);

        var points = _centerPoints.Values
            .Union(_nexusPoints.Values)
            .Union(_edgePoints.Values)
            .Union(_interiorPoints.Values);
        foreach (var point in points)
        {
            nav.Waypoints.Add(point.Id, point);
        }
        MergePoints(key.Data);
        SetLandWaypointProperties();
        MakePolyPaths();
        
        return report;
    }
    private void MakeCenterNavPoints(Data data)
    {
        _centerPoints = new Bijection<MapPolygon, Waypoint>();
        foreach (var poly in data.GetAll<MapPolygon>())
        {
            var point = Waypoint.Construct(_key, _id.GetID(), poly.Center, poly);
            if (poly.IsWater())
            {
                point.SetType(new SeaNav(), _key);
            }
            else
            {
                point.SetType(new InlandNav(), _key);
            }
            _centerPoints.Add(poly, point);
            data.Planet.Nav.MakeCenterPoint(poly, point, _key);
            
        }
    }
    private void MakeNexusNavPoints(Data data)
    {
        _nexusPoints = new Bijection<MapPolyNexus, Waypoint>();
        foreach (var nexus in data.GetAll<MapPolyNexus>())
        {
            var incidentPolys = nexus.IncidentPolys.Items(data);
            if (incidentPolys.Count() > 4)
            {
                throw new Exception();
            }
            var point = Waypoint.Construct(_key, _id.GetID(),
                nexus.Point,
                incidentPolys.First(),
                incidentPolys.Count() >= 2 ? incidentPolys.ElementAt(1) : null,
                incidentPolys.Count() >= 3 ? incidentPolys.ElementAt(2) : null,
                incidentPolys.Count() >= 4 ? incidentPolys.ElementAt(3) : null);
            
            if (nexus.IsRiverNexus(data))
            {
                point.SetType(new RiverNav(), _key);
            }
            else if (incidentPolys.Any(p => p.IsWater()))
            {
                if (incidentPolys.Any(p => p.IsLand))
                {
                    var seas = incidentPolys
                        .Where(p => p.IsWater())
                        .Select(p => _key.Data.Planet.PolygonAux.LandSea.SeaDic[p].Id)
                        .Distinct();
                    
                    point.SetType(CoastNav.Construct(seas.First()), _key);
                }
                else
                {
                    point.SetType(new SeaNav(), _key);
                }
            }
            else
            {
                point.SetType(new InlandNav(), _key);
            }
            _nexusPoints.Add(nexus, point);
        }
    }

    private bool ShouldMakeEdgePoint(MapPolygonEdge edge, Data data)
    {
        var segs = edge.HighSegsRel(data).Segments;
        if(segs.Sum(ls => ls.Length()) >= _nonRiverEdgeSplitLength)        return true;
        if (edge.IsRiver()) return false;

        if (edge.HiNexus.Entity(data).IsRiverNexus(data) 
            && edge.HiNexus.Entity(data).IsRiverNexus(data)) 
            return true;
        return false;
    }
    private void MakeEdgePoints(Data data)
    {
        _edgePoints = new Bijection<MapPolygonEdge, Waypoint>();
        foreach (var edge in data.GetAll<MapPolygonEdge>())
        {
            var segs = edge.HighSegsRel(data).Segments;
            var hi = edge.HighPoly.Entity(data);
            var lo = edge.LowPoly.Entity(data);
            
            if (ShouldMakeEdgePoint(edge, data) == false) continue;
            
            var pos = segs.GetPointAlong(.5f) + hi.Center;
            
            var point = Waypoint.Construct(_key, _id.GetID(),
                    pos, 
                    hi,
                    lo
                );
            var hiWater = hi.IsWater();
            var loWater = lo.IsWater();
            
            if (hiWater || loWater)
            {
                if (hiWater && loWater)
                {
                    point.SetType(new SeaNav(), _key);
                }
                else
                {
                    var waterPoly = hiWater ? hi : lo;
                    var sea = _key.Data.Planet.PolygonAux.LandSea.SeaDic[waterPoly];
                    point.SetType(CoastNav.Construct(sea.Id), _key);
                }
            }
            else if (edge.IsRiver())
            {
                point.SetType(new RiverNav(), _key);
            }
            else
            {
                point.SetType(new InlandNav(), _key);
            }
            
            _edgePoints.Add(edge, point);
        }
    }

    private void MakeInteriorPoints(Data data)
    {
        _interiorPoints = new Bijection<Vector2, Waypoint>();
        foreach (var mapPolygon in data.GetAll<MapPolygon>())
        {
            var chain = mapPolygon.GetEdges(data)
                .Select(e => e.GetSegsRel(mapPolygon, data)).Ordered<PolyBorderChain, Vector2>();
            var center = _centerPoints[mapPolygon];
            var land = mapPolygon.IsLand;
            for (var i = 0; i < chain.Count; i++)
            {
                var link = chain[i];
                var edge = link.GetEdge(data);
                
                var hi = _nexusPoints[edge.HiNexus.Entity(data)];
                var pHi = Waypoint.Construct(_key, _id.GetID(), 
                    GetInteriorPos(mapPolygon, hi), mapPolygon);
                pHi.SetType(land ? new InlandNav() : new SeaNav(), _key);
                var hiKey = new Vector2(center.Id, hi.Id);
                
                //why is it clashing sometimes?
                if(_interiorPoints.Contains(hiKey) == false) _interiorPoints.Add(hiKey, pHi);
                
                var lo = _nexusPoints[edge.LoNexus.Entity(data)];
                var pLo = Waypoint.Construct(_key, _id.GetID(), 
                    GetInteriorPos(mapPolygon, lo), mapPolygon);
                pLo.SetType(land ? new InlandNav() : new SeaNav(), _key);
                var loKey = new Vector2(center.Id, lo.Id);
                
                if(_interiorPoints.Contains(loKey) == false) _interiorPoints.Add(loKey, pLo);
                
                if (_edgePoints.Contains(edge))
                {
                    var edgePoint = _edgePoints[edge];
                    var pEdge = Waypoint.Construct(_key, _id.GetID(), 
                        GetInteriorPos(mapPolygon, edgePoint), mapPolygon);
                    pEdge.SetType(land ? new InlandNav() : new SeaNav(), _key);
                    _interiorPoints.Add(new Vector2(center.Id, edgePoint.Id), pEdge);
                }
            }
        }
    }
    private Vector2 GetInteriorPos(MapPolygon mapPolygon, Waypoint p)
    {
        return mapPolygon.Center + mapPolygon.GetOffsetTo(p.Pos, _key.Data) / 2f;
    }
    private void LinkPoints(Data data)
    {
        foreach (var poly in data.GetAll<MapPolygon>())
        {
            var center = _centerPoints[poly];
            
            foreach (var edge in poly.GetEdges(data))
            {
                var hi = _nexusPoints[edge.HiNexus.Entity(data)];
                var lo = _nexusPoints[edge.LoNexus.Entity(data)];
                var hiMid = _interiorPoints[new Vector2(center.Id, hi.Id)];
                var loMid = _interiorPoints[new Vector2(center.Id, lo.Id)];

                if (_edgePoints.TryGetValue(edge, out var edgePoint))
                {
                    var edgeMid = _interiorPoints[new Vector2(center.Id, edgePoint.Id)];
                    linkX(hi, edgePoint, hiMid, edgeMid);
                    linkTri(center, hiMid, edgeMid);

                    linkX(lo, edgePoint, loMid, edgeMid);
                    linkTri(center, loMid, edgeMid);
                }
                else
                {
                    linkX(hi, lo, hiMid, loMid);
                    linkTri(center, loMid, hiMid);
                }
            }

            void linkX(Waypoint p1, Waypoint p2, Waypoint p3, Waypoint p4)
            {
                addEdge((p1, p2));
                addEdge((p1, p3));
                addEdge((p1, p4));
                
                addEdge((p2, p3));
                addEdge((p2, p4));
                
                addEdge((p3, p4));
            }

            void linkTri(Waypoint p1, Waypoint p2, Waypoint p3)
            {
                addEdge((p1, p2));
                addEdge((p3, p2));
                addEdge((p1, p3));
            }

            void addEdge((Waypoint p, Waypoint o) pair)
            {
                pair.o.Neighbors.Add(pair.p.Id);
                pair.p.Neighbors.Add(pair.o.Id);
            }
        }
    }

    private void MergePoints(Data data)
    {
        var removed = new HashSet<int>();
        MergeOcean(removed, data);
        // MergeClose(removed, data);
        foreach (var i in removed)
        {
            data.Planet.Nav.Waypoints.Remove(i);
        }
    }

    private IEnumerable<Waypoint> GetInteriors(MapPolygon poly, Data data)
    {
        var center = _centerPoints[poly];
        var nexusInteriorWps = poly.GetNexi(data)
            .Select(n => _interiorPoints[new Vector2(center.Id, _nexusPoints[n].Id)]);
        var edgeInteriorWps = poly.GetEdges(data)
            .Where(e => _edgePoints.Contains(e))
            .Select(e => _interiorPoints[new Vector2(center.Id, _edgePoints[e].Id)]);
        return nexusInteriorWps.Union(edgeInteriorWps);
    }

    private void MergeClose(HashSet<int> removed, Data data)
    {
        foreach (var poly in data.GetAll<MapPolygon>())
        {
            if (poly.IsWater()) continue;
            var center = _centerPoints[poly];
            foreach (var nPoly in poly.Neighbors.Items(data))
            {
                var edge = poly.GetEdge(nPoly, data);
                var interiorHiWp = _nexusPoints[edge.HiNexus.Entity(data)];
                var interiorLoWp = _nexusPoints[edge.HiNexus.Entity(data)];
                if (_edgePoints.TryGetValue(edge, out var edgeWp))
                {
                    var interiorKey = new Vector2(center.Id, edgeWp.Id);
                    var interiorEdgeWp = _interiorPoints[interiorKey];
                    
                    if (data.Planet.GetOffsetTo(interiorHiWp.Pos, interiorLoWp.Pos).Length() < 100f)
                    {
                        GD.Print("merging at " + poly.Id);
                        var dHi = data.Planet
                            .GetOffsetTo(interiorHiWp.Pos, interiorEdgeWp.Pos).Length();
                        var dLo = data.Planet
                            .GetOffsetTo(interiorLoWp.Pos, interiorEdgeWp.Pos).Length();
                        var close = dHi < dLo ? interiorHiWp : interiorLoWp;
                        MergePoint(close, interiorEdgeWp, removed);
                    }
                }
            }
        }
    }
    private void MergeOcean(HashSet<int> removed, Data data)
    {
        foreach (var poly in data.GetAll<MapPolygon>())
        {
            if (poly.IsLand) continue;
            var center = _centerPoints[poly];
            var interiors = GetInteriors(poly, data);
            foreach (var interior in interiors)
            {
                MergePoint(center, interior, removed);
            }
        }
        
        foreach (var edge in data.GetAll<MapPolygonEdge>())
        {
            if (_edgePoints.Contains(edge) == false) continue;
            var edgeWp = _edgePoints[edge];
            if (edgeWp.WaypointData.Value() is SeaNav == false) continue;
            var hi = _centerPoints[edge.HighPoly.Entity(data)];
            MergePoint(hi, edgeWp, removed);
        }
        
        foreach (var nexus in data.GetAll<MapPolyNexus>())
        {
            var nexusWp = _nexusPoints[nexus];
            if (nexusWp.WaypointData.Value() is SeaNav == false) continue;
            var hi = nexus.IncidentPolys.Items(data)
                .OrderByDescending(i => i.Id).First();
            var hiWp = _centerPoints[hi];
            MergePoint(hiWp, nexusWp, removed);
        }
    }
    
    private void MergePoint(Waypoint mergingPoint, Waypoint toMerge, HashSet<int> removed)
    {
        foreach (var n in toMerge.Neighbors)
        {
            var nWaypoint = _key.Data.Planet.Nav.Waypoints[n];
            nWaypoint.Neighbors.Remove(toMerge.Id);
            mergingPoint.Neighbors.Add(n);
            nWaypoint.Neighbors.Add(mergingPoint.Id);
        }
        toMerge.Neighbors.Clear();
        removed.Add(toMerge.Id);
    }

    private void SetLandWaypointProperties()
    {
        var nav = _key.Data.Planet.Nav;
        var waypoints = nav.Waypoints;
        foreach (var waypoint in waypoints.Values)
        {
            if (waypoint.WaypointData.Value() is LandNav n == false) continue;
            var pos = _key.Data.Planet.ClampPosition(waypoint.Pos);
            var poly = getWaypointPolys(waypoint).First();
            var offset = poly.GetOffsetTo(waypoint.Pos, _key.Data);
            var pt = poly.Tris.Tris
                .OrderBy(t => t.GetCentroid().DistanceTo(offset)).First();
            n.SetRoughness(pt.Landform(_key.Data).MinRoughness, _key);
        }

        IEnumerable<MapPolygon> getWaypointPolys(Waypoint wp)
        {
            if (_interiorPoints.Contains(wp))
            {
                var centerId = (int)_interiorPoints[wp].X;
                var polyWp = nav.Waypoints[centerId];
                return _centerPoints[polyWp].Yield();
            }
            else if (_centerPoints.Contains(wp))
            {
                return _centerPoints[wp].Yield();
            }
            else if (_nexusPoints.Contains(wp))
            {
                var nexus = _nexusPoints[wp];
                return nexus.IncidentPolys.Items(_key.Data);
            }
            else if (_edgePoints.Contains(wp))
            {
                var edge = _edgePoints[wp];
                return new[] { edge.HighPoly.Entity(_key.Data), edge.LowPoly.Entity(_key.Data) };
            }

            throw new Exception();
        }
    }
    
    private void MakePolyPaths()
    {
        var nav = _key.Data.Planet.Nav;
        foreach (var edge in _key.Data.GetAll<MapPolygonEdge>())
        {
            var hi = edge.HighPoly.Entity(_key.Data);
            var lo = edge.LowPoly.Entity(_key.Data);
            var path = PathFinder.FindNavPath(hi, lo, _key.Data);
            if (path != null)
            {
                if (path.Count > 1)
                {
                    nav.PolyNavPaths.Add(new Vector2(hi.Id, lo.Id), 
                        path.Select(w => w.Id).ToList());
                }
                else if (path.Count == 1)
                {
                    GD.Print("size 1 path");
                }
            }
            else
            {
                throw new Exception();
            }
        }
    }
}
