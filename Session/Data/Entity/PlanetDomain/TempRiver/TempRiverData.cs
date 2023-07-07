using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class TempRiverData
{
    public ConcurrentDictionary<PolyCornerKey, Vector2> Inners { get; private set; }
    public ConcurrentDictionary<EdgeEndKey, Vector2> HiPivots { get; private set; }
    public ConcurrentDictionary<MapPolygon, MapPolyRiverTriInfoOld> Infos { get; private set; }
    public TempRiverData()
    {
        HiPivots = new ConcurrentDictionary<EdgeEndKey, Vector2>();
        Inners = new ConcurrentDictionary<PolyCornerKey, Vector2>();
        Infos = new ConcurrentDictionary<MapPolygon, MapPolyRiverTriInfoOld>();
    }

    public void GenerateInfos(GenWriteKey key)
    {
        var polys = key.Data.Planet.Polygons.Entities;
        
        // polys.ToList().ForEach(poly =>
        // {
        //     if (poly.GetEdges(key.Data).Any(e => e.IsRiver()) == false) return;
        //     var info = new MapPolyRiverTriInfo(poly, this, key);
        //     Infos.TryAdd(poly, info);
        // });
        
        try
        {
            Parallel.ForEach(polys, poly =>
            {
                if (poly.GetEdges(key.Data).Any(e => e.IsRiver()) == false) return;
                var info = new MapPolyRiverTriInfoOld(poly, this, key);
                Infos.TryAdd(poly, info);
            });
        }
        catch
        {
            throw;
        }
        
    }
}

public class MapPolyRiverTriInfoOld
{
    public MapPolygon Poly { get; private set; }
    public Dictionary<EdgeEndKey, PolyTri> InnerTris { get; private set; }
    public Dictionary<MapPolygonEdge, List<PolyTri>> BankTris { get; private set; }
    public List<PolyTri> LandTris { get; private set; }
    public Dictionary<MapPolygonEdge, List<LineSegment>> BankSegs { get; private set; }
    public List<LineSegment> InnerBoundary { get; private set; }
    public MapPolyRiverTriInfoOld(MapPolygon poly, TempRiverData rData, GenWriteKey key)
    {
        Poly = poly;
        InnerTris = new Dictionary<EdgeEndKey, PolyTri>();
        BankTris = new Dictionary<MapPolygonEdge, List<PolyTri>>();
        BankSegs = new Dictionary<MapPolygonEdge, List<LineSegment>>();
        InnerBoundary = new List<LineSegment>();
        LandTris = new List<PolyTri>();
        var edges = poly.GetEdges(key.Data);
        if (edges.Any(e => e.IsRiver()) == false) return;
        var nexi = edges.Select(e => e.HiNexus.Entity(key.Data))
            .Union(edges.Select(e2 => e2.LoNexus.Entity(key.Data)))
            .Distinct()
            .Where(n => n.IncidentPolys.Contains(poly))
            .ToHashSet();
        
        MakePivotTris(key.Data, rData, nexi, edges);
        MakeBankTris(key.Data, rData, edges);
        MakeInnerBoundary(key.Data, rData, nexi, edges);
        MakeLandTris(rData, key);
    }


    private void MakePivotTris(Data data, TempRiverData rData, HashSet<MapPolyNexus> nexi,
        IEnumerable<MapPolygonEdge> edges)
    {
        foreach (var nexus in nexi)
        {
            if (nexus.IsRiverNexus(data) == false) continue;
            var nexusPoint = Poly.GetOffsetTo(nexus.Point, data);
            var nexusEdges = edges.Where(e => nexus.IncidentEdges.Contains(e));
            
            var innerKey = new PolyCornerKey(nexus, Poly);
            var inner = rData.Inners[innerKey];
            
            var edge1 = nexusEdges.ElementAt(0);
            if (edge1.IsRiver() == false)
            {
                var endKey1 = new EdgeEndKey(nexus, edge1);
                var pivot1 = GetPivot(endKey1, rData, data);
                InnerTris.Add(endKey1, PolyTri.Construct(Poly.Id, nexusPoint, inner, pivot1, 
                    LandformManager.River, VegetationManager.Barren));
            }

            if (nexusEdges.Count() > 1)
            {
                var edge2 = nexusEdges.ElementAt(1);
                if (edge2.IsRiver() == false)
                {
                    var endKey2 = new EdgeEndKey(nexus, edge2);
                    var pivot2 = GetPivot(endKey2, rData, data);
                    InnerTris.Add(endKey2, PolyTri.Construct(Poly.Id, nexusPoint, inner, pivot2, 
                        LandformManager.River, VegetationManager.Barren));
                }
            }
        }
    }
    private void MakeBankTris(Data data, TempRiverData rData,
        IEnumerable<MapPolygonEdge> edges)
    {
        var borderPoints = Poly.GetOrderedBoundaryPoints(data);
        foreach (var edge in edges)
        {
            if (edge.IsRiver() == false) continue;
            var width = River.GetWidthFromFlow(edge.MoistureFlow);
            
            var hiNexus = edge.HiNexus.Entity(data);
            var hiNexusPoint = Poly.GetOffsetTo(hiNexus.Point, data);
            var hiEnd = new EdgeEndKey(hiNexus, edge);
            var hiCorner = new PolyCornerKey(hiNexus, Poly);
            var hiInner = rData.Inners[hiCorner];
            
            var loNexus = edge.LoNexus.Entity(data);
            var loNexusPoint = Poly.GetOffsetTo(loNexus.Point, data);
            var loEnd = new EdgeEndKey(loNexus, edge);
            var loCorner = new PolyCornerKey(loNexus, Poly);
            var loInner = rData.Inners[loCorner];
            
            var edgeSegs = edge.GetSegsRel(Poly, data).Segments;
            var firstEdgeP = edgeSegs[0].From;
            var lastEdgeP = edgeSegs[edgeSegs.Count - 1].To;
            Vector2 startInner;
            Vector2 endInner;
            if (loNexusPoint == firstEdgeP && hiNexusPoint == lastEdgeP)
            {
                startInner = loInner;
                endInner = hiInner;
            }
            else if (hiNexusPoint == firstEdgeP && loNexusPoint == lastEdgeP)
            {
                startInner = hiInner;
                endInner = loInner;
            }
            else throw new Exception();
            
            var bankPoints = new List<Vector2>();
            var bankSegs = new List<LineSegment>();
            
            for (var i = 0; i < edgeSegs.Count - 1; i++)
            {
                var thisSeg = edgeSegs[i];
                var nextSeg = edgeSegs[i + 1];
                if (thisSeg.To != nextSeg.From) throw new Exception();
                var thisSegAxis = (thisSeg.To - thisSeg.From).Normalized();
                var thisShift = thisSegAxis.Rotated(Mathf.Pi / 2f) * width / 2f;
                var bankPoint = thisSeg.To + thisShift;
                if (Poly.PointInPolyRel(bankPoint, data) == false)
                {
                    bankPoint = thisSeg.To.Normalized() * (thisSeg.To.Length() - width);
                    if (Poly.PointInPolyRel(bankPoint, data) == false) throw new Exception();
                }
                bankPoints.Add(bankPoint);
            }

            
            if (bankPoints.Count > 0)
            {
                bankSegs.Add(new LineSegment(startInner, bankPoints.First()));
                for (var i = 0; i < bankPoints.Count - 1; i++)
                {
                    bankSegs.Add(new LineSegment(bankPoints[i], bankPoints[i + 1]));
                }
                bankSegs.Add(new LineSegment(bankPoints.Last(), endInner));
            }
            else
            {
                bankSegs.Add(new LineSegment(startInner, endInner));
            }
            
            BankSegs.Add(edge, bankSegs);
            if (edgeSegs.Count != bankSegs.Count) throw new Exception();

            var bankTris = new List<PolyTri>();
            for (var i = 0; i < edgeSegs.Count; i++)
            {
                var outSeg = edgeSegs[i];
                var inSeg = bankSegs[i];
                bankTris.Add(PolyTri.Construct(Poly.Id, outSeg.From, outSeg.To, inSeg.From,
                    LandformManager.River, VegetationManager.Barren));
                bankTris.Add(PolyTri.Construct(Poly.Id, outSeg.To, inSeg.To, inSeg.From,
                    LandformManager.River, VegetationManager.Barren));
            }
            BankTris.Add(edge, bankTris);
        }
    }

    private void MakeInnerBoundary(Data data, TempRiverData rData, HashSet<MapPolyNexus> nexi,
        IEnumerable<MapPolygonEdge> edges)
    {
        var edgeInners = new List<List<LineSegment>>();

        foreach (var edge in edges)
        {
            var edgeInner = new List<LineSegment>();
            if (edge.IsRiver())
            {
                edgeInner.AddRange(BankSegs[edge]);
            }
            else if (edge.HiNexus.Entity(data).IsRiverNexus(data) == false && edge.LoNexus.Entity(data).IsRiverNexus(data) == false)
            {
                edgeInner.AddRange(edge.GetSegsRel(Poly, data).Segments);
            }
            else
            {
                var hiNexusP = Poly.GetOffsetTo(edge.HiNexus.Entity(data).Point, data);
                var loNexusP = Poly.GetOffsetTo(edge.LoNexus.Entity(data).Point, data);
                
                var edgeSegs = edge.GetSegsRel(Poly, data).Segments;

                MapPolyNexus fromNexus;
                MapPolyNexus toNexus;

                if (edgeSegs.Count > 1 && edgeSegs.First().From == edgeSegs[1].From)
                {
                    throw new Exception();
                }
                
                var from = edgeSegs.First().From;
                var to = edgeSegs.Last().To;
                if (hiNexusP == from
                    && loNexusP == to)
                {
                    fromNexus = edge.HiNexus.Entity(data);
                    toNexus = edge.LoNexus.Entity(data);
                }
                else if (hiNexusP == to
                         && loNexusP == from)
                {
                    toNexus = edge.HiNexus.Entity(data);
                    fromNexus = edge.LoNexus.Entity(data);
                } else { throw new Exception("bad epsilon"); }

                Vector2 continueFrom = from;
                Vector2 continueTo = to;
                
                LineSegment firstInnerEdge = null;
                var edgePoints = edgeSegs.GetPoints();
                var epsilon = .01f;
                if (fromNexus.IsRiverNexus(data))
                {
                    var fromInner = rData.Inners[new PolyCornerKey(fromNexus, Poly)];
                    var fromPivotSource = GetPivot(new EdgeEndKey(fromNexus, edge), rData, data);
                    var fromPivot = edgePoints.OrderBy(p => p.DistanceTo(fromPivotSource)).First();
                    
                    if (fromPivotSource.DistanceTo(fromPivot) > epsilon)
                    {
                        GD.Print("source pivot " + fromPivotSource);
                        GD.Print("closest pivot " + fromPivot);
                        throw new Exception();
                    }
                    
                    var fromPivotSeg = edgeSegs.First(ep => ep.To == fromPivot);
                    fromPivot = fromPivotSeg.To;
                    firstInnerEdge = new LineSegment(fromInner, fromPivot);
                    continueFrom = fromPivot;
                }

                LineSegment lastInnerEdge = null;
                if (toNexus.IsRiverNexus(data))
                {
                    var toInner = rData.Inners[new PolyCornerKey(toNexus, Poly)];
                    var toPivotSource = GetPivot(new EdgeEndKey(toNexus, edge), rData, data);
                    var toPivot = edgePoints.OrderBy(p => p.DistanceTo(toPivotSource)).First();
                    if (toPivotSource.DistanceTo(toPivot) > epsilon)
                    {
                        GD.Print("source pivot " + toPivotSource);
                        GD.Print("closest pivot " + toPivot);
                        throw new Exception();
                    }
                    var toPivotSeg = edgeSegs.First(ep => ep.From == toPivot);
                    lastInnerEdge = new LineSegment(toPivot, toInner);
                    continueTo = toPivot;
                }

                var continueFromIndex = edgeSegs.FindIndex(ls => ls.From == continueFrom);
                var continueToIndex = edgeSegs.FindIndex(ls => ls.To == continueTo);
                
                if(firstInnerEdge != null) edgeInner.Add(firstInnerEdge);
                for (var i = continueFromIndex; i <= continueToIndex; i++)
                {
                    edgeInner.Add(edgeSegs[i]);
                }
                if(lastInnerEdge != null && lastInnerEdge.IsSame(firstInnerEdge) == false)
                {
                    edgeInner.Add(lastInnerEdge);
                }
            }
            edgeInner = edgeInner.Chainify();
            edgeInners.Add(edgeInner);
        }
        
        InnerBoundary = edgeInners.Chainify();
        InnerBoundary.CompleteCircuit();

        try
        {
            InnerBoundary.GetPoints().ToArray().PolyTriangulate(data, Poly);
        }
        catch
        {
            var e = new GeometryException("p2t failed for river poly inner boundary");
            var bPoints = Poly.GetOrderedBoundaryPoints(data);
            var bSegs = Poly.GetOrderedBoundarySegs(data);
            
            
            e.AddSegLayer(InnerBoundary, "inner boundary");
            e.AddSegLayer(bSegs, "poly boundary");
            e.AddSegLayer(InnerBoundary.Union(Poly.GetOrderedBoundarySegs(data)).ToList(),
                "both");
            var rEdges = Poly.GetEdges(data).Where(edge => edge.IsRiver());
                
            var rSegs = rEdges.SelectMany(edge => edge.GetSegsRel(Poly, data).Segments).ToList();
            var rWidth = rEdges.Average(edge => River.GetWidthFromFlow(edge.MoistureFlow));

            var ingraved = Geometry2D.OffsetPolygon(bPoints, -rWidth / 2f).Cast<Vector2[]>()
                .SelectMany(vs => vs.ToList().GetLineSegments());
            e.AddSegLayer(ingraved.Union(bSegs).ToList(), "ingraved");
            
                
                
            

            
            var inners = new List<Vector2>();
            foreach (var nexus in Poly.GetNexi(data))
            {
                var key = new PolyCornerKey(nexus, Poly);
                if(rData.Inners.TryGetValue(key, out var inner)) inners.Add(inner);
            }
            e.AddPointSet(inners, "inners");
            throw e;
        }
    }

    private void MakeLandTris(TempRiverData rData, GenWriteKey key)
    {
        LandTris = InnerBoundary.GetPoints().ToArray().PolyTriangulate(key.Data, Poly);
    }
    private Vector2 GetPivot(EdgeEndKey key, TempRiverData rData, Data data)
    {
        var pivot = rData.HiPivots[key];
        if (Poly != key.Edge.HighPoly.Entity(data))
        {
            pivot += Poly.GetOffsetTo(key.Edge.HighPoly.Entity(data), data);
        }
        return pivot;
    }
    
}
