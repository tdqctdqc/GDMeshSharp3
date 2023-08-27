using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DelaunatorSharp;

public class PolygonGenerator : Generator
{
    private Vector2 _dimensions;
    private bool _leftRightWrap;
    private float _polySize;
    private List<Vector2> _innerPoints;
    private Data _data;
    public PolygonGenerator(List<Vector2> innerPoints, Vector2 dimensions, 
                                    bool leftRightWrap, float polySize)
    {
        _innerPoints = innerPoints;
        _dimensions = dimensions;
        _leftRightWrap = leftRightWrap;
        _polySize = polySize;
    }
    public override GenReport Generate(GenWriteKey key)
    {
        var report = new GenReport(GetType().Name);
        _data = key.Data;
        
        report.StartSection();
        var info = new MapGenInfo(_innerPoints, _dimensions, _polySize, _leftRightWrap);
        key.GenData.GenInfo = info;
        if (info.Points.Any(v => v != v.Intify()))
        {
            throw new Exception();
        }
        report.StopSection("Setup");

        
        report.StartSection();
        var delaunayPoints = info.Points.Select(p => new Triangulator.DelaunatorPoint(p)).ToList<IPoint>();
        CreateAndRegisterPolys(delaunayPoints, info, key);
        report.StopSection("Creating points and polys");

        report.StartSection();
        var graph = GraphGenerator.GenerateMapPolyVoronoiGraph(info, key);
        report.StopSection("Generating poly graph");

        if (_leftRightWrap)
        {
            report.StartSection();
            Wrap(graph, info, key);
            report.StopSection("Wrapping");
        }
        else
        {
            throw new NotImplementedException();
        }
        report.StartSection();
        BuildBorders(info, graph, key);
        report.StopSection("Building borders");
        return report;
    }

    private void CreateAndRegisterPolys(IEnumerable<IPoint> points, MapGenInfo info, GenWriteKey key)
    {
        var polys = new List<MapPolygon>(points.Count());
        foreach (var dPoint in points)
        {
            var center = dPoint.GetIntV2();
            var polygon = MapPolygon.Create(center, _dimensions.X, key);
        }
        
        info.SetupPolys(key.Data.GetAll<MapPolygon>().ToList());
    }

    private void Wrap(Graph<MapPolygon, LineSegment> graph, MapGenInfo info, GenWriteKey key)
    {
        var wrapLeft = new List<MapPolygon>();
        wrapLeft.Add(info.CornerPolys[0]);
        wrapLeft.AddRange(info.LeftPolys);
        wrapLeft.Add(info.CornerPolys[2]);
            
        var wrapRight = new List<MapPolygon>();
        wrapRight.Add(info.CornerPolys[1]);
        wrapRight.AddRange(info.RightPolys);
        wrapRight.Add(info.CornerPolys[3]);
            
        GraphGenerator.WrapMapPolygonGraph(graph, wrapLeft, wrapRight, key);
    }
    private void BuildBorders(MapGenInfo info, Graph<MapPolygon, LineSegment> graph, GenWriteKey key)
    {
        var rHash = new HashSet<MapPolygon>(info.RightPolys);
        rHash.Add(info.CornerPolys[1]);
        rHash.Add(info.CornerPolys[3]);

        var borderChains = new ConcurrentDictionary<PolyBorderChain, PolyBorderChain>();
        var ps = graph.Elements.ToList();
        ps.ForEach(p => BuildBorderSegs(p, info, rHash, graph, key, borderChains));
        
        var mapWidth = key.GenData.GenMultiSettings.Dimensions.X;
        var nexusPoints = new Dictionary<Vector2, List<MapPolygonEdge>>();
        
        foreach (var b in borderChains)
        {
            CreateEdgeAndBorderChains(b, key, mapWidth, nexusPoints);
        }
        
        ps.ForEach(p => FlipEdgeSegsToClockwise(p));
        

        var edgeNexi = new Dictionary<MapPolygonEdge, Vector2>();
        
        CreateNexi(nexusPoints, edgeNexi, key);
        BindNexiToEdges(edgeNexi, key);
        
        key.Data.Notices.SetPolyShapes.Invoke();
    }
    
    private void FlipEdgeSegsToClockwise(MapPolygon poly)
    {
        // GD.Print(poly.Id);
        var borders = poly.GetPolyBorders();
        
        var allEdgeSegs = borders.SelectMany(b => b.Segments).ToList();
        if (allEdgeSegs.Count == 0) throw new Exception();
        // GD.Print(poly.Id + " 1");
        try
        {
            allEdgeSegs = allEdgeSegs.FlipChainify();

        }
        catch (Exception e)
        {
            var ex = new GeometryException($"couldnt build poly {poly.Id} borders");
            ex.AddSegLayer(allEdgeSegs, "border segs");
            ex.AddPointSet(poly.Neighbors.Items(_data).Select(n => poly.GetOffsetTo(n, _data)).ToList(), "neighbors");
        }

        
        
        var sum = allEdgeSegs.GetAngleAroundSum(Vector2.Zero);

        if (sum == 0f) throw new Exception();
        if (sum < 0f) allEdgeSegs = allEdgeSegs.Select(e => e.Reverse()).ToList();
        
        foreach (var border in borders)
        {
            if (border.Segments.Count != 1) throw new Exception();
            var edgeSeg = border.Segments[0];
            var newSeg = allEdgeSegs.First(ls => ls.IsSame(edgeSeg) || ls.Reverse().IsSame(edgeSeg));
            if (newSeg.IsSame(edgeSeg) == false)
            {
                edgeSeg.From = newSeg.From;
                edgeSeg.To = newSeg.To;
            }
        }
    }
    private void BuildBorderSegs(MapPolygon mp, MapGenInfo info, HashSet<MapPolygon> rHash,
        Graph<MapPolygon, LineSegment> graph, GenWriteKey key, 
        ConcurrentDictionary<PolyBorderChain, PolyBorderChain> borderChains)
    {
        if (info.LRWrap && rHash.Contains(mp))
        {
            throw new Exception();
        }
        var neighbors = graph.GetNeighbors(mp).Where(n => rHash.Contains(n) == false).ToList();
        if (neighbors.Count == 0) throw new Exception();
            
        neighbors.ForEach(nMp =>
        {
            if (nMp.Id > mp.Id) return;
            var edge = graph.GetEdge(mp, nMp);
            if (edge.From == edge.To)
            {
                return;
            }

            if (edge.From != edge.From.Intify() || edge.To != edge.To.Intify())
            {
                throw new Exception();
            }
                

            var lowEdge = new LineSegment(nMp.GetOffsetTo(edge.From, key.Data), 
                nMp.GetOffsetTo(edge.To, key.Data));
            if (lowEdge.IsCCW(Vector2.Zero))
            {
                lowEdge = lowEdge.Reverse();
            }
            var highEdge = new LineSegment(mp.GetOffsetTo(edge.From, key.Data), 
                mp.GetOffsetTo(edge.To, key.Data));
            if (highEdge.IsCCW(Vector2.Zero))
            {
                highEdge = highEdge.Reverse();
            }
            var chain1 = MapPolygonEdge.ConstructBorderChain(mp, nMp,
                new List<LineSegment> {highEdge}, key.Data);
                
                
            var chain2 = MapPolygonEdge.ConstructBorderChain(nMp, mp,
                new List<LineSegment> {lowEdge}, key.Data);
            borderChains.TryAdd(chain1, chain2);
        });
    }

    private void CreateEdgeAndBorderChains(KeyValuePair<PolyBorderChain, PolyBorderChain> b, GenWriteKey key,
        float mapWidth, Dictionary<Vector2, List<MapPolygonEdge>> nexuses)
    {
        var hiChain = b.Key;
        var loChain = b.Value;
        var edge = MapPolygonEdge.Create(b.Key, b.Value, key);

        var start = hiChain.Segments.First().From + b.Key.Native.Entity(key.Data).Center;
        if (start != start.Intify()) throw new Exception();
            
        var end = hiChain.Segments.Last().To + b.Key.Native.Entity(key.Data).Center;
        if (end != end.Intify()) throw new Exception();

        if (start.X < 0) start.X += mapWidth;
        if (end.X < 0) end.X += mapWidth;
            
        if(nexuses.ContainsKey(start) == false) nexuses.Add(start, new List<MapPolygonEdge>());
        if(nexuses.ContainsKey(end) == false) nexuses.Add(end, new List<MapPolygonEdge>());
            
        nexuses[start].Add(edge);
        nexuses[end].Add(edge);
    }

    private void CreateNexi(Dictionary<Vector2, List<MapPolygonEdge>> nexuses,
        Dictionary<MapPolygonEdge, Vector2> edgeNexi, GenWriteKey key)
    {
        foreach (var kvp in nexuses)
        {
            var point = kvp.Key;
            var edges = kvp.Value;
            var polys = edges.Select(e => e.HighPoly.Entity(key.Data))
                .Union(edges.Select(e => e.LowPoly.Entity(key.Data)))
                .Distinct().ToList();
            var nexus = MapPolyNexus.Create(point, edges, polys, key);
            foreach (var e in edges)
            {
                if (edgeNexi.ContainsKey(e) == false) edgeNexi.Add(e, Vector2.Zero);
                if (edgeNexi[e].X == 0) edgeNexi[e] = new Vector2(nexus.Id, 0);
                else
                {
                    edgeNexi[e] = new Vector2(edgeNexi[e].X, nexus.Id);
                }
            }
        }
    }

    private void BindNexiToEdges(Dictionary<MapPolygonEdge, Vector2> edgeNexi, GenWriteKey key)
    {
        foreach (var kvp in edgeNexi)
        {
            var edge = kvp.Key;
            var n1 = key.Data.Get<MapPolyNexus>((int)kvp.Value.X);
            var n2 = key.Data.Get<MapPolyNexus>((int)kvp.Value.Y);
            edge.SetNexi(n1, n2, key);
        }
    }
}
