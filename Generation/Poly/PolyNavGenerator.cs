using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyNavGenerator : Generator
{
    private GenWriteKey _key;
    private IdDispenser _id;
    private Dictionary<MapPolygon, Waypoint> _centerPoints;
    private Dictionary<MapPolygonEdge, Waypoint> _edgePoints;
    private Dictionary<MapPolyNexus, Waypoint> _nexusPoints;
    private Dictionary<Vector2, Waypoint> _interiorPoints;
    public override GenReport Generate(GenWriteKey key)
    {
        _key = key;
        var report = new GenReport(nameof(PolyNavGenerator));
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
        
        return report;
    }
    private void MakeCenterNavPoints(Data data)
    {
        _centerPoints = new Dictionary<MapPolygon, Waypoint>();
        foreach (var poly in data.GetAll<MapPolygon>())
        {
            var point = Waypoint.Construct(_key, _id.GetID(), poly, poly.Center);
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
        _nexusPoints = new Dictionary<MapPolyNexus, Waypoint>();
        foreach (var nexus in data.GetAll<MapPolyNexus>())
        {
            var point = Waypoint.Construct(_key, _id.GetID(), 
                nexus.IncidentPolys.Items(data).First(),
                nexus.Point);
            if(nexus.IncidentEdges.Items(data)
               .Any(e => point.Pos == e.HighPoly.Entity(data).Center || point.Pos == e.LowPoly.Entity(data).Center))
            {
                GD.Print("in nexus");
            }
            if (nexus.IncidentPolys.Items(data).Any(p => p.IsWater()))
            {
                if (nexus.IncidentPolys.Items(data).Any(p => p.IsLand))
                {
                    point.SetType(new CoastNav(), _key);
                }
                else
                {
                    point.SetType(new SeaNav(), _key);
                }
            }
            else if (nexus.IsRiverNexus(data))
            {
                point.SetType(new RiverNav(), _key);
            }
            else
            {
                point.SetType(new InlandNav(), _key);
            }
            _nexusPoints.Add(nexus, point);
        }
    }

    private void MakeEdgePoints(Data data)
    {
        _edgePoints = new Dictionary<MapPolygonEdge, Waypoint>();
        foreach (var edge in data.GetAll<MapPolygonEdge>())
        {
            var segs = edge.HighSegsRel(data).Segments;
            if (segs.Sum(ls => ls.Length()) < 100f) continue;
            var pos = segs.GetPointAlong(.5f) + edge.HighPoly.Entity(data).Center;
            if (pos == edge.HighPoly.Entity(data).Center || pos == edge.LowPoly.Entity(data).Center)
            {
                GD.Print("in edge");
            }
            var point = Waypoint.Construct(_key, _id.GetID(), 
                edge.HighPoly.Entity(data),
                pos);
            
            if (edge.HighPoly.Entity(data).IsWater())
            {
                if (edge.LowPoly.Entity(data).IsLand)
                {
                    point.SetType(new CoastNav(), _key);
                }
                else
                {
                    point.SetType(new SeaNav(), _key);
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
        _interiorPoints = new Dictionary<Vector2, Waypoint>();
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
                var pHi = Waypoint.Construct(_key, _id.GetID(), mapPolygon, 
                    GetInteriorPos(mapPolygon, hi));
                pHi.SetType(land ? new InlandNav() : new SeaNav(), _key);
                _interiorPoints.TryAdd(new Vector2(center.Id, hi.Id), pHi);
                if (pHi.Pos == edge.HighPoly.Entity(data).Center || pHi.Pos == edge.LowPoly.Entity(data).Center)
                {
                    GD.Print("interior hi");
                }
                
                var lo = _nexusPoints[edge.LoNexus.Entity(data)];
                var pLo = Waypoint.Construct(_key, _id.GetID(), mapPolygon, 
                    GetInteriorPos(mapPolygon, lo));
                pLo.SetType(land ? new InlandNav() : new SeaNav(), _key);
                if (pLo.Pos == edge.HighPoly.Entity(data).Center || pLo.Pos == edge.LowPoly.Entity(data).Center)
                {
                    GD.Print("interior lo");
                }
                
                _interiorPoints.TryAdd(new Vector2(center.Id, lo.Id), pLo);
                
                if (_edgePoints.ContainsKey(edge))
                {
                    var edgePoint = _edgePoints[edge];
                    var pEdge = Waypoint.Construct(_key, _id.GetID(), mapPolygon, 
                        GetInteriorPos(mapPolygon, edgePoint));
                    pEdge.SetType(land ? new InlandNav() : new SeaNav(), _key);
                    _interiorPoints.Add(new Vector2(center.Id, edgePoint.Id), pEdge);
                    if (pEdge.Pos == edge.HighPoly.Entity(data).Center || pEdge.Pos == edge.LowPoly.Entity(data).Center)
                    {
                        GD.Print("interior");
                    }
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
        var centerPoints = _centerPoints.Values.ToHashSet();
        var nexusPoints = _nexusPoints.Values.ToHashSet();
        var edgePoints = _edgePoints.Values.ToHashSet();
        var interiorPoints = _interiorPoints.Values.ToHashSet();
        var points = data.Planet.Nav.Waypoints.Values.ToList();
        void mergePoints(Waypoint mergePoint, params Waypoint[] toMerge)
        {
            
        }
    }

    private void MergeOceanPoints(MapPolygon poly)
    {
        
    }

}
