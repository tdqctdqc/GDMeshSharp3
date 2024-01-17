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
        public List<PolyTri> Tris { get; private set; }
        public Vector2[] InsetPoints { get; set; }

        public RiverTriGenScratch()
        {
            NexiCloseIndices = new Dictionary<MapPolyNexus, int>();
            InnerEdgeIndices = new Dictionary<MapPolygonEdge, Vector2>();
            Tris = new List<PolyTri>();
        }
    }
    public static (List<PolyTri>, PolyCell[]) DoPoly(MapPolygon poly, Data data, TempRiverData rData, GenWriteKey key)
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
        return (scratch.Tris, cells.ToArray());
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

        MakeLandCells(poly, cells, innerBoundarySegs, scratch.Tris, key);
        var landCells = cells.OfType<LandCell>();
        var newRiverCells = MergeRiverCells(cells, key);
        var allCells = ((IEnumerable<PolyCell>)landCells)
            .Union(newRiverCells).ToList();
        
        PolyCell.ConnectCellsByEdge(landCells, newRiverCells, poly.Center,
            (v, w) =>
            {
                v.Neighbors.Add(w.Id);
                w.Neighbors.Add(v.Id);
            }, data);
        PolyCell.ConnectCellsSharingPoints(newRiverCells, key.Data);

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
                    var riverEdge = (polyNexusRiverEdges.Count() > 0)
                        ? polyNexusRiverEdges.First()
                        : edge;

                    var t = PolyTri.Construct(poly.Id, corner, pivot, close,
                        data.Models.Landforms.River, data.Models.Vegetations.Barren);

                    var newSeg = new LineSegment(end ? pivot : close,
                        end ? close : pivot);
                    innerBoundarySegs.Add(newSeg);
                
                    var cell = RiverCell.Construct(riverEdge, poly.Center, 
                        new Vector2[] { t.A, t.B, t.C }, key);
                    cells.Add(cell);
                    scratch.Tris.Add(t);
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
            var bankTriPs = Geometry2D.TriangulatePolygon(riverBPoints);
            for (var i = 0; i < bankTriPs.Length; i+=3)
            {
                var a = riverBPoints[bankTriPs[i]];
                var b = riverBPoints[bankTriPs[i + 1]];
                var c = riverBPoints[bankTriPs[i + 2]];
                var t = PolyTri.Construct(poly.Id, a, b, c,
                    data.Models.Landforms.River,
                    data.Models.Vegetations.Barren);
                scratch.Tris.Add(t);
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
    private static List<RiverCell> MergeRiverCells(
        List<PolyCell> cells, GenWriteKey key)
    {
        var riverCellsByEdge = cells.OfType<RiverCell>().SortInto(c => c.Edge.RefId);
        var newRiverCells = new List<RiverCell>();
        foreach (var kvp in riverCellsByEdge)
        {
            if (kvp.Value.Count > 3) throw new Exception();
            if (kvp.Value.Count == 3)
            {
                var triCells = kvp.Value.Where(c => c.RelBoundary.Count() == 3);
                if (triCells.Count() != 2) throw new Exception();
                var big = kvp.Value.Except(triCells).First();
                foreach (var tCell in triCells)
                {
                    var newBounds = Geometry2D.MergePolygons(tCell.RelBoundary, big.RelBoundary);
                    if (newBounds.Count() != 1)
                    {
                        //SET ISSUE
                        continue;
                    }
                    big.SetBoundary(newBounds.First(), key);
                }
                newRiverCells.Add(big);
            }
            if (kvp.Value.Count == 2)
            {
                var union = Geometry2D.MergePolygons(
                    kvp.Value[0].RelBoundary,
                    kvp.Value[1].RelBoundary);
                if (union.Count() != 1)
                {
                    //DO ISSUE
                    continue;
                }

                var c1 = kvp.Value[0];
                c1.SetBoundary(union.First(), key);
                newRiverCells.Add(c1);
                // GD.Print(union.Count);
            }
            else if (kvp.Value.Count == 1)
            {
                newRiverCells.Add(kvp.Value.First());
            }
        }

        return newRiverCells;
    }
    private static void MakeLandCells(MapPolygon poly,
        List<PolyCell> cells,
        HashSet<LineSegment> innerBoundarySegs,
        List<PolyTri> tris, GenWriteKey key)
    {
        try
        {
            Vector2[] innerBoundaryPs = innerBoundarySegs.ToList().Chainify().GetPoints().ToArray();
            tris.AddRange(innerBoundaryPs.PolyTriangulate(key.Data, poly));
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
                tris.AddRange(innerBoundaryPs.PolyTriangulate(key.Data, poly));
                cells.AddRange(GraphGenerator.GenerateAndConnectPolyCellsForInterior(poly, innerBoundaryPs, key));
                var a = badTri[0].From;
                var b = badTri[0].To;
                Vector2 c = badTri[1].From != a && badTri[1].From != b
                    ? badTri[1].From
                    : badTri[1].To;
                    
                var center = (a + b + c) / 3f;
                tris.Add(PolyTri.Construct(poly.Id, a,b,c, 
                    key.Data.Models.Landforms.River, key.Data.Models.Vegetations.Barren
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
