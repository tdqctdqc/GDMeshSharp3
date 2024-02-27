using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DelaunatorSharp;
using Godot;
using System.Collections.Generic;
using VoronoiSandbox;

public static class PreCellGenerator
{
    
    public static PreCellResult Make(Vector2I dim, GenWriteKey key)
    {
        var result = new PreCellResult();
        
        var bounds = new Vector2[]
        {
            Vector2.Zero,
            Vector2.Right * dim.X,
            new Vector2(dim.X, dim.Y),
            Vector2.Down * dim.Y
        };
        var total = new Stopwatch();
        total.Start();
        var sw = new Stopwatch();
        
        sw.Start();
        var (points, dummyPoints) 
            = MakeCellPoints(30, dim);
        sw.Stop();
        GD.Print($"make points {sw.Elapsed.TotalMilliseconds}");
        sw.Reset();
        
        sw.Start();
        var iPoints = points
            .Select(p => p.GetIPoint()).ToArray();
        var delaunator = new Delaunator(iPoints);
        sw.Stop();
        GD.Print($"delaunator {sw.Elapsed.TotalMilliseconds}");
        sw.Reset();

        sw.Start();
        var graph = delaunator.GetVoronoiGraphNew(result, dim, key);
        sw.Stop();
        GD.Print($"make graph {sw.Elapsed.TotalMilliseconds}");
        sw.Reset();
        
        sw.Start();
        var cells = MakeCells(dim, points, graph, bounds, 
            dummyPoints, key);
        sw.Stop();
        GD.Print($"make cells {sw.Elapsed.TotalMilliseconds}");
        sw.Reset();
        
        
        
        sw.Start();
        MergeLeftRight(cells, dim);
        sw.Stop();
        GD.Print($"merge left right {sw.Elapsed.TotalMilliseconds}");
        sw.Reset();
        
        sw.Start();
        Parallel.ForEach(cells, c => c.MakePointsAbs(dim));
        sw.Stop();
        GD.Print($"making cell abs points {sw.Elapsed.TotalMilliseconds}");
        sw.Reset();
        
        sw.Start();
        var polys = MakePolys(cells, dim, key);
        sw.Stop();
        GD.Print($"make polys {sw.Elapsed.TotalMilliseconds}");
        sw.Reset();
        
        sw.Start();
        CheckPolysContiguous(polys, dim, key);
        sw.Stop();
        GD.Print($"check polys contiguous {sw.Elapsed.TotalMilliseconds}");
        sw.Reset();

        foreach (var poly in polys)
        {
            foreach (var cell in poly.Cells)
            {
                if (cell.PrePoly != poly) throw new Exception();
            }
        }
        
        
        sw.Start();
        MakePolyNeighbors(polys);
        sw.Stop();
        GD.Print($"make poly neighbors {sw.Elapsed.TotalMilliseconds}");
        sw.Reset();
        
        sw.Start();
        var edges = MakeEdges(polys, key);
        sw.Stop();
        GD.Print($"make edges {sw.Elapsed.TotalMilliseconds}");
        sw.Reset();
        
        sw.Start();
        var nexi = MakeNexi(polys, cells, 
            edges, dim, result, key);
        sw.Stop();
        GD.Print($"make nexi {sw.Elapsed.TotalMilliseconds}");
        sw.Reset();

        result.Nexi = nexi;
        result.Cells = cells;
        result.Polys = polys;
        result.Edges = edges;
        
        total.Stop();
        GD.Print("total " + total.Elapsed.TotalMilliseconds);
        return result;
    }

    


    private static (List<Vector2I> cellPoints,
        HashSet<Vector2I> dummyPoints) MakeCellPoints(
        int pointDist,
        Vector2I dim)
    {
        var points = new List<Vector2I>();
        var xCells = Mathf.FloorToInt((float)dim.X / pointDist);
        var yCells = Mathf.FloorToInt((float)dim.Y / pointDist);
        for (int i = 0; i <= xCells; i++)
        {
            for (int j = 0; j <= yCells; j++)
            {
                var center = new Vector2(i * pointDist, j * pointDist);
                var offsetRadius = .6f * pointDist;
                if (i == 0 || i == xCells 
                           || j == 0 || j == yCells)
                {
                    offsetRadius = 0f;
                }
                else if (i == 1 || i == xCells - 1
                                || j == 1 || j == yCells - 1)
                {
                    offsetRadius = pointDist / 2f;
                }

                var offset = new Vector2(Random.Shared.NextSingle() * offsetRadius,
                    Random.Shared.NextSingle() * offsetRadius);
                var relTo = center + offset;
                if (i == xCells) relTo.X = dim.X;
                if (j == yCells) relTo.Y = dim.Y;
                
                points.Add((Vector2I)relTo.Intify());
            }
        }

        var dummyPoints = new HashSet<Vector2I>();
        for (var i = 0; i <= xCells; i++)
        {
            var x = i * pointDist;
            var yUp = -pointDist * 5;
            var yDown = dim.Y + pointDist * 5;
            points.Add(new Vector2I(x, yUp));
            points.Add(new Vector2I(x, yDown));
            dummyPoints.Add(new Vector2I(x, yUp));
            dummyPoints.Add(new Vector2I(x, yDown));
        }
        for (var i = 0; i <= yCells; i++)
        {
            var y = i * pointDist;
            var xRight = pointDist * 5 + dim.X;
            var xLeft = -pointDist * 5;
            points.Add(new Vector2I(xLeft, y));
            points.Add(new Vector2I(xRight, y));
            dummyPoints.Add(new Vector2I(xLeft, y));
            dummyPoints.Add(new Vector2I(xRight, y));
        }
        return (points, dummyPoints);
    }
    private static List<Vector2I> MakePolyPoints(
        float pointDist,
        Vector2I dim,
        GenWriteKey key)
    {
        var points = new List<Vector2I>();
        var xCells = Mathf.CeilToInt(dim.X / pointDist);
        var yCells = Mathf.CeilToInt(dim.Y / pointDist);

        
        for (int i = 0; i < xCells; i++)
        {
            for (int j = 0; j < yCells; j++)
            {
                var center = new Vector2(i * pointDist, j * pointDist);
                var offsetRadius = pointDist;
                
                var offset = new Vector2(Random.Shared.NextSingle() * offsetRadius,
                    Random.Shared.NextSingle() * offsetRadius);
                if (i % 2 == 0) offset += Vector2.Left * pointDist / 2f;
                var point = (center + offset).ClampPosition(key.Data);
                points.Add((Vector2I)(center + offset).Intify());
            }
        }
        
        return points;
    }
    
    
    private static List<PreCell> MakeCells(Vector2I dim, 
        List<Vector2I> points,
        (Vector2I, Vector2I, (Vector2I, Vector2I))[] graph, 
        Vector2[] bounds, HashSet<Vector2I> dummyPoints,
        GenWriteKey key)
    {
        var cellDic = points
            .Where(p => dummyPoints.Contains(p) == false)
            .Select(p => (p, new PreCell(key.Data.IdDispenser.TakeId(), p)))
            .ToDictionary(p => p.p,
                p => p.Item2);
        
        foreach (var (v1, v2, edge)
                 in graph)
        {
            if (dummyPoints.Contains(v1) 
                || dummyPoints.Contains(v2))
            {
                continue;
            }
            var c1 = cellDic[v1];
            var c2 = cellDic[v2];
            
            var e = (edge.Item1.Clamp(Vector2I.Zero, dim), edge.Item2.Clamp(Vector2I.Zero, dim));
            c1.AddNeighborAbs(c2, e, dim);
            c2.AddNeighborAbs(c1, e, dim);
        }
        
        
        return cellDic.Values.ToList();
    }

    private static void MergeLeftRight(List<PreCell> cells, 
        Vector2I dim)
    {
        var lefts = cells.Where(c => c.RelTo.X == 0f)
            .OrderBy(c => c.RelTo.Y)
            .ToList();
        var rights = cells.Where(c => c.RelTo.X == dim.X)
            .OrderBy(c => c.RelTo.Y)
            .ToList();
        if (lefts.Count != rights.Count)
        {
            throw new Exception($"lefts {lefts.Count} rights {rights.Count}");
        }
        for (var i = 0; i < lefts.Count; i++)
        {
            var left = lefts[i];
            var right = rights[i];
            if (left.RelTo.Y != right.RelTo.Y)
            {
                throw new Exception();
            }
            for (var j = 0; j < left.Neighbors.Count; j++)
            {
                var leftN = left.Neighbors[j];
                if (leftN.RelTo.X == 0f) continue;
                var leftEdge = left.EdgesRel[j];
                leftN.ReplaceNeighbor(left, right);
                right.AddNeighborRel(leftN, leftEdge);
            }
        }
        for (var i = 0; i < lefts.Count - 1; i++)
        {
            var left = lefts[i];
            var nextLeft = lefts[i + 1];
            var leftEdge = left.EdgeWith(nextLeft);
            var leftP = leftEdge.Item1.X == 0f 
                ? leftEdge.Item2 
                : leftEdge.Item1;
            
            var right = rights[i];
            var nextRight = rights[i + 1];
            var rightEdge = right.EdgeWith(nextRight);


            var rightP = rightEdge.Item1.X == 0f 
                ? rightEdge.Item2 
                : rightEdge.Item1;
            var newRightEdge = (leftP, rightP);
            var rightOffset = right.RelTo - nextRight.RelTo;
            var newNextRightEdge = (leftP + rightOffset, rightP + rightOffset);
            right.ReplaceEdgeRel(nextRight, newRightEdge);
            nextRight.ReplaceEdgeRel(right, newNextRightEdge);
        }

        cells.RemoveAll(c => c.RelTo.X == 0f);
    }


    private static List<PrePoly> MakePolys(List<PreCell> cells, 
        Vector2I dim, GenWriteKey key)
    {
        var points = MakePolyPoints(150f, dim, key);
        int idIndex = 0;
        
        var polyGrid = new CylinderGrid<PrePoly>(
            dim, 100f, 
            p => p.RelTo);
        
        foreach (var p in points)
        {
            var poly = new PrePoly(key.Data.IdDispenser.TakeId(), 
                p.ClampPosition(dim));
            polyGrid.Add(poly);
        }
        var polyCells = cells
            .AsParallel()
            .Select(cell =>
            {
                var found = polyGrid.TryGetClosest(cell.RelTo,
                    out var poly, p => true);
                if (found == false) throw new Exception();
                // cell.PrePoly = poly;
                return (cell, poly);
            }).ToDictionary(kvp => kvp.cell, kvp => kvp.poly);
        foreach (var (cell, poly) in polyCells)
        {
            if (cell.PrePoly != null) throw new Exception();
            cell.PrePoly = poly;
            poly.Cells.Add(cell);
        }

        return polyGrid.Cells
            .SelectMany(c => c.Value)
            .Where(p => p.Cells.Count > 0).ToList();
    }

    private static void CheckPolysContiguous(List<PrePoly> polys, 
        Vector2I dim, GenWriteKey key)
    {

        var queue = new Queue<PreCell>();
        var hash = new HashSet<PreCell>();
        var nonContiguous = new HashSet<PrePoly>();
        foreach (var poly in polys)
        {
            hash.Clear();
            queue.Clear();
            queue.Enqueue(poly.Cells[0]);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                hash.Add(c);
                foreach (var nCell in c
                             .Neighbors
                             .Where(n => n.PrePoly == poly
                                 && hash.Contains(n) == false))
                {
                    hash.Add(nCell);
                    queue.Enqueue(nCell);
                }
            }

            if (hash.Count != poly.Cells.Count)
            {
                // throw new Exception();
                nonContiguous.Add(poly);
            }
        }
        GD.Print($"non contiguous {nonContiguous.Count} / {polys.Count}");

        foreach (var poly in nonContiguous)
        {
            var unions = UnionFind.Find(poly.Cells,
                (c, d) => true, c => c.Neighbors);
            var biggest = unions.MaxBy(l => l.Count);
            foreach (var union in unions)
            {
                if (union == biggest) continue;
                foreach (var cell in union)
                {
                    var newPoly = cell.Neighbors.Select(n => n.PrePoly)
                        .Where(n => nonContiguous.Contains(n) == false)
                        .MinBy(n => n.RelTo.Offset(cell.RelTo, key.Data).Length());
                    if (newPoly == null)
                    {
                        GD.Print("no new poly found at " + cell.RelTo);
                        continue;
                    }

                    poly.Cells.Remove(cell);
                    newPoly.Cells.Add(cell);
                    cell.PrePoly = newPoly;
                }
            }
        }
    }
    

    private static void MakePolyNeighbors(List<PrePoly> polys)
    {
        Parallel.ForEach(polys, poly =>
        {
            var ns = poly.Cells
                .SelectMany(c => c.Neighbors.Select(n => n.PrePoly))
                .Where(n => n != poly)
                .ToHashSet();
            poly.Neighbors = ns;
        });
    }
    
    private static Dictionary<Vector2I, PreEdge> MakeEdges(
        List<PrePoly> polys, GenWriteKey key)
    {
        int idIndex = 0;
        var edges = new Dictionary<Vector2I, PreEdge>();
        foreach (var poly in polys)
        {
            foreach (var nPoly in poly.Neighbors)
            {
                var edgeKey = poly.Id.GetIdEdgeKey(nPoly.Id);
                if (edges.ContainsKey(edgeKey))
                {
                    continue;
                }
                edges.Add(edgeKey, 
                    new PreEdge(key.Data.IdDispenser.TakeId(), poly, nPoly));
            }
        }
        return edges;
    }

    private static List<PreNexus> MakeNexi(List<PrePoly> polys,
        List<PreCell> cells, 
        Dictionary<Vector2I, PreEdge> edges, Vector2I dim,
        PreCellResult preCellResult, GenWriteKey key)
    {
        var pointAbsDic = new Dictionary<Vector2I, (PreCell X, PreCell Y, PreCell Z)>();
        var borderCells = cells
            .Where(c => c.Neighbors.Any(n => n.PrePoly != c.PrePoly));
        
        foreach (var cell in borderCells)
        {
            foreach (var pAbs in cell.PointsAbs)
            {
                pointAbsDic.TryAdd(pAbs, default);
                if (pointAbsDic[pAbs].X == null)
                {
                    pointAbsDic[pAbs] = (cell, pointAbsDic[pAbs].Y, pointAbsDic[pAbs].Z);
                }
                else if (pointAbsDic[pAbs].Y == null)
                {
                    pointAbsDic[pAbs] = (pointAbsDic[pAbs].X, cell, pointAbsDic[pAbs].Z);
                }
                else if (pointAbsDic[pAbs].Z == null)
                {
                    pointAbsDic[pAbs] = (pointAbsDic[pAbs].X, pointAbsDic[pAbs].Y, cell);
                }
                else
                {
                    throw new Exception();
                }
            }
        }

        int idIter = 0;

        var nexi = pointAbsDic
            .AsParallel()
            .Select(v =>
            {
                var (pAbs, (ci1, ci2, ci3)) = v;
                
                if (ci1 == null) return null;
                if (ci2 == null) return null;
                var c1 = ci1;
                var c2 = ci2;
                if (c1.PrePoly == c2.PrePoly) return null;
                var nexus = new PreNexus(key.Data.IdDispenser.TakeId(), pAbs);
                if (ci3 == null)
                {
                    nexus.E1 = edges[c1.PrePoly.Id.GetIdEdgeKey(c2.PrePoly.Id)];
                    nexus.P1 = c1.PrePoly;
                    nexus.P2 = c2.PrePoly;
                }
                else
                {
                    var c3 = ci3;
                    if (c3.PrePoly == c1.PrePoly || c3.PrePoly == c2.PrePoly)
                        return null;
                    nexus.P1 = c1.PrePoly;
                    nexus.P2 = c2.PrePoly;
                    nexus.P3 = c3.PrePoly;
                    var ek1 = c1.PrePoly.Id.GetIdEdgeKey(c2.PrePoly.Id);
                    var ek2 = c2.PrePoly.Id.GetIdEdgeKey(c3.PrePoly.Id);
                    var ek3 = c3.PrePoly.Id.GetIdEdgeKey(c1.PrePoly.Id);
                    nexus.E1 = edges[ek1];
                    nexus.E2 = edges[ek2];
                    nexus.E3 = edges[ek3];
                }
                return nexus;
            })
            .Where(n => n is not null)
            .ToList();
        
        foreach (var nexus in nexi)
        {
            nexus.E1?.SetNexus(nexus);
            nexus.E2?.SetNexus(nexus);
            nexus.E3?.SetNexus(nexus);
        }
        
        foreach (var kvp in edges)
        {
            var edge = kvp.Value;
            if (edge.N1 == null) throw new Exception();
            if (edge.N2 == null) throw new Exception();
        }
        
        return nexi;
    }
}