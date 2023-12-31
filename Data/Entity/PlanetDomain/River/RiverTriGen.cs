using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class RiverTriGen
{

    public static List<PolyTri> DoPoly(MapPolygon poly, Data data, TempRiverData rData, GenWriteKey key)
    {
        var nexiCloseIndices = new Dictionary<MapPolyNexus, int>();
        var innerEdgeIndices = new Dictionary<MapPolygonEdge, Vector2>();
        var tris = new List<PolyTri>();

        var edges = poly.GetEdges(data);
        var rEdges = edges.Where(e => e.IsRiver());
        var rWidth = 0f;
        if(rEdges.Count() > 0) rWidth = rEdges.Average(e => River.GetWidthFromFlow(e.MoistureFlow));
        if(rWidth == 0f)
        {
            rWidth = poly.GetNexi(data)
                .SelectMany(n => n.IncidentEdges.Items(data))
                .Max(e => River.GetWidthFromFlow(e.MoistureFlow));
            if (rWidth == 0f) throw new Exception("making river info for non-river poly");
        }
        
        var bPoints = poly.GetOrderedBoundaryPoints(data);
        
        var insetsSource = (Vector2[])Geometry2D.OffsetPolygon(bPoints, -rWidth / 2f)[0];
        var insetPoints = insetsSource.Where((v, i) => v.DistanceTo(insetsSource.Modulo(i + 1)) > 1f).ToArray();

        var minEdgeLength = 10f;
        var oldInset = insetPoints.ToList();

        bool fixedIt = false;
        if (edges.Any(e => e.GetLength(data) <= minEdgeLength))
        {
            var t = ConstrainInsetForShortEdges(poly, edges, data, insetPoints);
            if (t != insetPoints) fixedIt = true;
            insetPoints = t;
        }
        
        MakeInnerEdges(poly, data, nexiCloseIndices, innerEdgeIndices, insetPoints);
        MakeTris(poly, data, rData, nexiCloseIndices, innerEdgeIndices,
            tris, insetPoints, key);
        
        return tris;
    }

    private static Vector2[] ConstrainInsetForShortEdges(MapPolygon poly, IEnumerable<MapPolygonEdge> edges,
        Data data, Vector2[] insetSource)
    {
        var minEdgeLength = 10f;
        var insetSourceSegs = insetSource.ToList().GetLineSegments(true).ToList();
        var shortEdges = edges.Where(e => e.GetLength(data) <= minEdgeLength).Select(e => e.GetSegsRel(poly, data));
        var shortEdgeChains = shortEdges.ChainSort<PolyBorderChain, Vector2>();
        
        var newInsetSegs = new List<LineSegment>();
        var excludeSegs = new HashSet<LineSegment>();

        var newInset = insetSource.ToList();
        for (var i = 0; i < shortEdgeChains.Count; i++)
        {
            var chain = shortEdgeChains[i];
            
            var from = chain.First().Segments.First().From;
            var fromClose = newInset.OrderBy(p => from.DistanceTo(p)).First();
            var fromCloseIndex = newInset.IndexOf(fromClose);
            
            var to = chain.Last().Segments.Last().To;
            var toClose = newInset.OrderBy(p => to.DistanceTo(p)).First();

            if (toClose != fromClose) continue;
            var axis = (to - from).Normalized();
            var perp = -axis.Orthogonal();
            newInset.RemoveAt(fromCloseIndex);
            var shiftToFrom = fromClose - axis * 2f + perp * 2f;
            var shiftToTo = toClose + axis * 2f + perp * 2f;;
            newInset.Insert(fromCloseIndex, shiftToFrom);
            newInset.Insert(fromCloseIndex + 1, shiftToTo);
            newInset.RemoveAll(v => v.DistanceTo(from) < shiftToFrom.DistanceTo(from));
            newInset.RemoveAll(v => v.DistanceTo(to) < shiftToTo.DistanceTo(to));
        }
        return newInset.ToArray();
    }
    private static void MakeInnerEdges(MapPolygon poly, Data data, Dictionary<MapPolyNexus, int> nexiCloseIndices, 
        Dictionary<MapPolygonEdge, Vector2> innerEdgeIndices, Vector2[] insetPoints)
    {
        var edges = poly.GetEdges(data);
        var rEdges = edges.Where(e => e.IsRiver());

        var nexi = poly.GetNexi(data);
        foreach (var nexus in nexi)
        {
            var point = poly.GetOffsetTo(nexus.Point, data);
            var dist = Mathf.Inf;
            var index = -1;
            for (var i = 0; i < insetPoints.Length; i++)
            {
                var p = insetPoints[i];
                var newDist = point.DistanceTo(p);
                if (newDist < dist)
                {
                    dist = newDist;
                    index = i;
                }
            }

            if (index == -1) throw new Exception();
            nexiCloseIndices.Add(nexus, index);
        }
        
        //check overlap?
        foreach (var rEdge in rEdges)
        {
            var fromTo = rEdge.OrderNexi(poly, data);
            var fromIndex = nexiCloseIndices[fromTo.from];
            var toIndex = nexiCloseIndices[fromTo.to];
            if (fromIndex == toIndex)
            {
                innerEdgeIndices.Add(rEdge, new Vector2(-1, -1));
                continue;
            }
            innerEdgeIndices.Add(rEdge, new Vector2(fromIndex, toIndex)); 
        }
    }

    private static void MakeTris(MapPolygon poly, Data data, TempRiverData rData,
        Dictionary<MapPolyNexus, int> nexiCloseIndices, Dictionary<MapPolygonEdge, Vector2> innerEdgeIndices,
        List<PolyTri> tris, Vector2[] insetPoints, GenWriteKey key)
    {
        var innerBoundarySegs = new HashSet<LineSegment>();
        var edges = poly.GetEdges(data);
        var bPoints = poly.GetOrderedBoundaryPoints(data);
        //a. make pivot tris and bank tris
        foreach (var edge in edges)
        {
            var edgeSegs = edge.GetSegsRel(poly, data).Segments;
            var fromTo = edge.OrderNexi(poly, data);
            
            if(fromTo.from.IsRiverNexus(data) == false && fromTo.to.IsRiverNexus(data) == false)
            {
                innerBoundarySegs.AddRange(edgeSegs);
                continue;
            }

            var fromPivot = edgeSegs[0].To; 
            var fromClose = insetPoints[nexiCloseIndices[fromTo.from]];
            var toPivot = edgeSegs[edgeSegs.Count - 1].From;
            var toClose = insetPoints[nexiCloseIndices[fromTo.to]];
            
            if (fromTo.from.IsRiverNexus(data))
            {
                if (edge.IsRiver() == false)
                {
                    innerBoundarySegs.Add(new LineSegment(fromClose, fromPivot));
                }
                tris.Add(PolyTri.Construct(poly.Id, edgeSegs[0].From, fromPivot, fromClose, 
                    data.Models.Landforms.River, data.Models.Vegetations.Barren));
            }
            else
            {
                innerBoundarySegs.Add(edgeSegs[0]);
            }
            if (fromTo.to.IsRiverNexus(data))
            {
                if (edge.IsRiver() == false) innerBoundarySegs.Add(new LineSegment(toPivot, toClose));
                tris.Add(PolyTri.Construct(poly.Id, edgeSegs[edgeSegs.Count - 1].To, toPivot, toClose, 
                    data.Models.Landforms.River, data.Models.Vegetations.Barren));
            }
            else
            {
                innerBoundarySegs.Add(edgeSegs[edgeSegs.Count - 1]);
            }

            var innerEdgeHash = new HashSet<int>();
            if (edge.IsRiver())
            {
                var thisEdgeInnerIndices = innerEdgeIndices[edge];
                var from = (int)thisEdgeInnerIndices.X;
                var to = (int)thisEdgeInnerIndices.Y;
                bool add = from != -1 && to != -1 && from != to;
                int iter = 0;
                if (from < to)
                {
                    iter = to - from;
                }
                else
                {
                    iter = to + (insetPoints.Length - from);
                }
                var curr = to;
                
                var riverPortionBoundary = new List<LineSegment>{};
                if (add)
                {
                    riverPortionBoundary.Add(new LineSegment(edgeSegs[edgeSegs.Count - 1].To,
                        insetPoints[to]));
                }
                while (add && iter > 0)
                {
                    iter--;
                    var prev = (curr - 1 + insetPoints.Length) % insetPoints.Length;
                    if(innerEdgeHash.Contains(curr) == false)
                    {
                        innerEdgeHash.Add(curr);
                        innerBoundarySegs.Add(new LineSegment(insetPoints[prev], insetPoints[curr]));
                    }
                    riverPortionBoundary.Add(new LineSegment(insetPoints[curr], insetPoints[prev]));
                    curr = prev;
                    if (curr == to) break;
                }
                if(add) riverPortionBoundary.Add(new LineSegment(insetPoints[from], edgeSegs[0].From));
                
                
                
                riverPortionBoundary.AddRange(edgeSegs);
                var riverBPoints = riverPortionBoundary.GetPoints().ToArray();

                var bankTriPs = Geometry2D.TriangulatePolygon(riverBPoints);
                for (var i = 0; i < bankTriPs.Length; i+=3)
                {
                    var a = riverBPoints[bankTriPs[i]];
                    var b = riverBPoints[bankTriPs[i + 1]];
                    var c = riverBPoints[bankTriPs[i + 2]];
                    tris.Add(PolyTri.Construct(poly.Id, a,b,c, 
                        data.Models.Landforms.River, data.Models.Vegetations.Barren));
                }
            }
            else
            {
                for (var i = 1; i < edgeSegs.Count - 1; i++)
                {
                    innerBoundarySegs.Add(edgeSegs[i]);
                }
            }
            
        }
        
        
        
        
        try
        {
            Vector2[] innerBoundaryPs = innerBoundarySegs.ToList().Chainify().GetPoints().ToArray();
            tris.AddRange(innerBoundaryPs.PolyTriangulate(data, poly));
        }
        catch
        {
            var badTri = innerBoundarySegs.ToList().FindTri();
            if (badTri == null) throw new Exception();
            badTri.ForEach(ls => innerBoundarySegs.Remove(ls));

            try
            {
                Vector2[] innerBoundaryPs = innerBoundarySegs.ToList().Chainify().GetPoints().ToArray();
                tris.AddRange(innerBoundaryPs.PolyTriangulate(data, poly));
                var a = badTri[0].From;
                var b = badTri[0].To;
                Vector2 c = badTri[1].From != a && badTri[1].From != b
                    ? badTri[1].From
                    : badTri[1].To;
                    
                    
                var center = (a + b + c) / 3f;
                var lf = data.Models.Landforms.GetAtPoint(poly, center, data);
                var v = data.Models.Vegetations.GetAtPoint(poly, center, lf, data);
                tris.Add(PolyTri.Construct(poly.Id, a,b,c, 
                    data.Models.Landforms.River, data.Models.Vegetations.Barren
                    ));
            }
            catch
            {
                var ex = new GeometryException("failed to fix");
                ex.AddSegLayer(innerBoundarySegs.ToList(), "inner boundary");
                throw ex;
            }
        }
    }
}
