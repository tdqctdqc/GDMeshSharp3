using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class RiverTriGen
{
    private class RiverTriGenScratch
    {
        public Dictionary<MapPolyNexus, int> NexiCloseIndices { get; private set; }
        public Dictionary<MapPolygonEdge, Vector2> InnerEdgeIndices { get; private set; }
        public Vector2[] InsetPoints { get; set; }

        public RiverTriGenScratch()
        {
            NexiCloseIndices = new Dictionary<MapPolyNexus, int>();
            InnerEdgeIndices = new Dictionary<MapPolygonEdge, Vector2>();
        }
    }
    public static PolyCell[] DoPoly(MapPolygon poly, Data data, TempRiverData rData, GenWriteKey key)
    {
        var scratch = new RiverTriGenScratch();
       
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
        if (edges.Any(e => e.GetLength(data) <= minEdgeLength))
        {
            var t = ConstrainInsetForShortEdges(poly, edges, data, insetPoints);
            insetPoints = t;
        }

        scratch.InsetPoints = insetPoints;
        
        MakeInnerEdges(poly, data, scratch, insetPoints);
        var cells = MakeTris(poly, data, rData, scratch, key);
        return cells.ToArray();
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
    private static void MakeInnerEdges(MapPolygon poly, Data data, 
        RiverTriGenScratch scratch, Vector2[] insetPoints)
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
            scratch.NexiCloseIndices.Add(nexus, index);
        }
        
        //check overlap?
        foreach (var rEdge in rEdges)
        {
            var fromTo = rEdge.OrderNexi(poly, data);
            var fromIndex = scratch.NexiCloseIndices[fromTo.from];
            var toIndex = scratch.NexiCloseIndices[fromTo.to];
            if (fromIndex == toIndex)
            {
                scratch.InnerEdgeIndices.Add(rEdge, new Vector2(-1, -1));
                continue;
            }
            scratch.InnerEdgeIndices.Add(rEdge, new Vector2(fromIndex, toIndex)); 
        }
    }

    private static List<PolyCell> MakeTris(MapPolygon poly, 
        Data data, TempRiverData rData,
        RiverTriGenScratch scratch, GenWriteKey key)
    {
        var cells = new List<PolyCell>();
        var innerBoundarySegs = new HashSet<LineSegment>();
        var edges = poly.GetEdges(data);
        var bPoints = poly.GetOrderedBoundaryPoints(data);
        //a. make pivot tris and bank tris
        foreach (var edge in edges)
        {
            DoEdge(edge, poly, key, scratch, cells, innerBoundarySegs);
        }

        MakeLandCells(poly, cells, innerBoundarySegs, key);
        var landCells = cells.OfType<LandCell>();
        var riverCells = cells.OfType<RiverCell>();
        var allCells = ((IEnumerable<PolyCell>)landCells)
            .Union(riverCells).ToList();
        
        PolyCell.ConnectCellsByEdge(landCells, riverCells, poly.Center,
            (v, w) =>
            {
                v.Neighbors.Add(w.Id);
                w.Neighbors.Add(v.Id);
            }, data);
        PolyCell.ConnectCellsSharingPoints(riverCells, key.Data);
        return allCells;
    }

    private static void DoEdge(MapPolygonEdge edge, 
        MapPolygon poly,
        GenWriteKey key,
        RiverTriGenScratch scratch,
        List<PolyCell> cells, 
        HashSet<LineSegment> innerBoundarySegs)
    {
        var data = key.Data;
        var edgeSegs = edge.GetSegsRel(poly, data).Segments;
        var fromTo = edge.OrderNexi(poly, data);
        if(fromTo.from.IsRiverNexus(data) == false 
           && fromTo.to.IsRiverNexus(data) == false)
        {
            innerBoundarySegs.AddRange(edgeSegs);
            return;
        }

        var fromPivot = edgeSegs[0].To; 
        var fromClose = scratch.InsetPoints[scratch.NexiCloseIndices[fromTo.from]];
        var toPivot = edgeSegs[edgeSegs.Count - 1].From;
        var toClose = scratch.InsetPoints[scratch.NexiCloseIndices[fromTo.to]];
        
        doCorner(fromTo.from, edgeSegs[0],edgeSegs[0].From, fromClose, fromPivot, false);
        doCorner(fromTo.to, edgeSegs[edgeSegs.Count - 1], edgeSegs[edgeSegs.Count - 1].To, toClose, toPivot, true);
        void doCorner(MapPolyNexus nexus, LineSegment borderSeg,
            Vector2 corner, Vector2 close, Vector2 pivot, bool end)
        {
            if (nexus.IsRiverNexus(data))
            {
                if (edge.IsRiver() == false)
                {
                    var riverEdges = 
                        nexus.IncidentEdges.Items(data)
                            .Where(e => e.IsRiver());
                    var polyNexusRiverEdges = 
                        riverEdges.Where(e => e.EdgeToPoly(poly));
                    var polyNexusCoastEdges = nexus.IncidentEdges.Items(data)
                        .Where(e => e.IsCoast(key.Data));
                    if (polyNexusRiverEdges.Count() > 1) throw new Exception();
                    var riverEdge = edge;
                        
                        // (polyNexusRiverEdges.Count() > 1)
                        // ? polyNexusRiverEdges.First()
                        // : edge;

                    var newSeg = new LineSegment(end ? pivot : close,
                        end ? close : pivot);
                    innerBoundarySegs.Add(newSeg);
                
                    var cell = RiverCell.Construct(riverEdge, poly.Center, 
                        new Vector2[] { corner, pivot, close }, key);
                    cells.Add(cell);
                }
            }
            else
            {
                innerBoundarySegs.Add(borderSeg);
            }
        }
        
        
        var innerEdgeHash = new HashSet<int>();
        if (edge.IsRiver())
        {
            var thisEdgeInnerIndices = scratch.InnerEdgeIndices[edge];
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
                iter = to + (scratch.InsetPoints.Length - from);
            }
            var curr = to;
            
            var riverPortionBoundary = new List<LineSegment>{};
            if (add)
            {
                riverPortionBoundary.Add(new LineSegment(edgeSegs[edgeSegs.Count - 1].To,
                    scratch.InsetPoints[to]));
            }
            while (add && iter > 0)
            {
                iter--;
                var prev = (curr - 1 + scratch.InsetPoints.Length) % scratch.InsetPoints.Length;
                if(innerEdgeHash.Contains(curr) == false)
                {
                    innerEdgeHash.Add(curr);
                    innerBoundarySegs.Add(new LineSegment(scratch.InsetPoints[prev], scratch.InsetPoints[curr]));
                }
                riverPortionBoundary.Add(new LineSegment(scratch.InsetPoints[curr], scratch.InsetPoints[prev]));
                curr = prev;
                if (curr == to) break;
            }
            if(add) riverPortionBoundary.Add(new LineSegment(scratch.InsetPoints[from], edgeSegs[0].From));
            
            riverPortionBoundary.AddRange(edgeSegs);
            var riverBPoints = riverPortionBoundary.GetPoints().ToArray();
            var cell = RiverCell.Construct(edge, poly.Center, riverBPoints, key);
            cells.Add(cell);
        }
        else
        {
            for (var i = 1; i < edgeSegs.Count - 1; i++)
            {
                innerBoundarySegs.Add(edgeSegs[i]);
            }
        }
    }
    
    private static void MakeLandCells(MapPolygon poly,
        List<PolyCell> cells,
        HashSet<LineSegment> innerBoundarySegs,
        GenWriteKey key)
    {
        try
        {
            Vector2[] innerBoundaryPs = innerBoundarySegs.ToList().Chainify().GetPoints().ToArray();
            cells.AddRange(GraphGenerator.GenerateAndConnectPolyCellsForInterior(poly, innerBoundaryPs, key));
        }
        catch
        {
            var badTri = innerBoundarySegs.ToList().FindTri();
            if (badTri == null) throw new Exception();
            badTri.ForEach(ls => innerBoundarySegs.Remove(ls));
            try
            {
                Vector2[] innerBoundaryPs = innerBoundarySegs.ToList().Chainify().GetPoints().ToArray();
                cells.AddRange(GraphGenerator.GenerateAndConnectPolyCellsForInterior(poly, innerBoundaryPs, key));
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
