using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DelaunatorSharp;
using Godot;
using System.Collections.Generic;

public static class PreCellGenerator
{
    
    public static PreCellResult Make
        (Vector2I dim, GenWriteKey key)
    {
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
        var graph = delaunator.GetVoronoiGraphNew();
        sw.Stop();
        GD.Print($"make graph {sw.Elapsed.TotalMilliseconds}");
        sw.Reset();
        
        sw.Start();
        var cells = MakeCells(dim, points, graph, bounds, dummyPoints, key);
        sw.Stop();
        GD.Print($"make cells {sw.Elapsed.TotalMilliseconds}");
        sw.Reset();
        
        sw.Start();
        MergeLeftRight(cells, dim);
        sw.Stop();
        GD.Print($"merge left right {sw.Elapsed.TotalMilliseconds}");
        sw.Reset();
        
        
        
        sw.Start();
        var polys = MakePolys(cells, dim, key);
        sw.Stop();
        GD.Print($"make polys {sw.Elapsed.TotalMilliseconds}");
        sw.Reset();
        
        
        
        sw.Start();
        CheckPolysContiguous(polys, dim, key.Data);
        sw.Stop();
        GD.Print($"check polys contiguous {sw.Elapsed.TotalMilliseconds}");
        sw.Reset();

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
        var nexi = MakeNexiNew(polys, edges, key);
        sw.Stop();
        GD.Print($"make nexi {sw.Elapsed.TotalMilliseconds}");
        sw.Reset();
        
        
        total.Stop();
        GD.Print("total " + total.Elapsed.TotalMilliseconds);
        var result = new PreCellResult(cells, polys, edges, nexi);
        return result;
    }

    


    private static (List<Vector2> cellPoints,
        HashSet<Vector2> dummyPoints) MakeCellPoints(
        int pointDist,
        Vector2I dim)
    {
        var points = new List<Vector2>();
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
                
                points.Add((Vector2I)relTo);
            }
        }

        var dummyPoints = new HashSet<Vector2>();
        for (var i = 0; i <= xCells; i++)
        {
            var x = i * pointDist;
            var yUp = -pointDist * 5;
            var yDown = dim.Y + pointDist * 5;
            points.Add(new Vector2(x, yUp));
            points.Add(new Vector2(x, yDown));
            dummyPoints.Add(new Vector2(x, yUp));
            dummyPoints.Add(new Vector2(x, yDown));
        }
        for (var i = 0; i <= yCells; i++)
        {
            var y = i * pointDist;
            var xRight = pointDist * 5 + dim.X;
            var xLeft = -pointDist * 5;
            points.Add(new Vector2(xLeft, y));
            points.Add(new Vector2(xRight, y));
            dummyPoints.Add(new Vector2(xLeft, y));
            dummyPoints.Add(new Vector2(xRight, y));
        }
        return (points, dummyPoints);
    }
    private static List<Vector2> MakePolyPoints(
        float pointDist,
        Vector2I dim,
        Data d)
    {
        var points = new List<Vector2>();
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
                if (i % 2 == 0) offset += Vector2.Left * pointDist / 2;
                var point = (center + offset).ClampPosition(d);
                points.Add(center + offset);
            }
        }
        
        return points;
    }
    
    
    private static List<PreCell> MakeCells(Vector2I dim, 
        List<Vector2> points,
        (Vector2, Vector2, (Vector2, Vector2))[] newGraph, 
        Vector2[] bounds, HashSet<Vector2> dummyPoints,
        GenWriteKey key)
    {
        var cellDic = points
            .AsParallel()
            .Where(p => dummyPoints.Contains(p) == false)
            .Select((p, i) => (p, new PreCell(key, p)))
            .ToDictionary(p => p.p,
                p => p.Item2);
        
        foreach (var (v1, v2, edge)
                 in newGraph)
        {
            if (dummyPoints.Contains(v1) 
                || dummyPoints.Contains(v2))
            {
                continue;
            }
            var c1 = cellDic[v1];
            var c2 = cellDic[v2];
            
            var e = (edge.Item1.Clamp(Vector2.Zero, dim), edge.Item2.Clamp(Vector2.Zero, dim));
            c1.AddNeighborAbs(c2, e, dim);
            c2.AddNeighborAbs(c1, e, dim);
        }
        
        
        return cellDic.Values.ToList();
    }

    private static void MergeLeftRight(List<PreCell> cells, Vector2 dim)
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
            right.ReplaceEdge(nextRight, newRightEdge);
            nextRight.ReplaceEdge(right, newNextRightEdge);
        }

        cells.RemoveAll(c => c.RelTo.X == 0f);
    }


    private static List<PrePoly> MakePolys(List<PreCell> cells, 
        Vector2I dim, GenWriteKey key)
    {
        var points = MakePolyPoints(150f, dim, key.Data);
        
        var polyGrid = new CylinderGrid<PrePoly>(
            dim, 100f, p => p.RelTo);
        
        foreach (var p in points)
        {
            var poly = new PrePoly(key, p.ClampPosition(key.Data));
            polyGrid.Add(poly);
        }

        var polyCells = cells.AsParallel()
            .Select(cell =>
            {
                var found = polyGrid.TryGetClosest(cell.RelTo,
                    out var poly, p => true);
                if (found == false) throw new Exception();
                cell.PrePoly = poly;
                return (cell, poly);
            });
        foreach (var (cell, poly) in polyCells)
        {
            poly.Cells.Add(cell);
        }

        return polyGrid.Cells
            .SelectMany(c => c.Value)
            .Where(p => p.Cells.Count > 0).ToList();
    }

    private static void CheckPolysContiguous(
        List<PrePoly> polys, Vector2I dim, Data d)
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
                        .MinBy(n => n.RelTo.Offset(cell.RelTo, d).Length());
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
    
    private static Dictionary<Vector2I, PreEdge> MakeEdges(List<PrePoly> polys, GenWriteKey key)
    {
        var edges = new Dictionary<Vector2I, PreEdge>();
        foreach (var poly in polys)
        {
            foreach (var nPoly in poly.Neighbors)
            {
                if (nPoly.Id > poly.Id) continue;
                edges.Add(poly.GetIdEdgeKey(nPoly), new PreEdge(key, poly, nPoly));
            }
        }
        return edges;
    }

    private static List<PreNexus> MakeNexiNew(List<PrePoly> polys,
        Dictionary<Vector2I, PreEdge> edges, GenWriteKey key)
    {
        var res = new List<PreNexus>();
        var dic = new Dictionary<Vector2, List<PrePoly>>();
        foreach (var poly in polys)
        {
            var counts = new Dictionary<Vector2, int>();
            foreach (var cell in poly.Cells)
            {
                foreach (var p in cell.GetPointsAbs(key.Data))
                {
                    counts.AddOrSum(p.ClampPosition(key.Data), 1);
                }
            }

            var singles = counts.Where(kvp => kvp.Value == 1);
            foreach (var (p, count) in singles)
            {
                dic.AddOrUpdate(p, poly);
            }
        }
        
        
        foreach (var (point, incidentPolys) in dic)
        {
            if (incidentPolys.Count == 2)
            {
                var p1 = incidentPolys[0];
                var p2 = incidentPolys[1];
                var edge = edges[p1.GetIdEdgeKey(p2)];
                var nexus = new PreNexus(key, point, p1, p2, null, edge, null, null);
                edge.SetNexus(nexus);
                res.Add(nexus);
            }
            else if (incidentPolys.Count == 3)
            {
                var p1 = incidentPolys[0];
                var p2 = incidentPolys[1];
                var p3 = incidentPolys[2];
                var e1 = edges[p1.GetIdEdgeKey(p2)];
                var e2 = edges[p2.GetIdEdgeKey(p3)];
                var e3 = edges[p3.GetIdEdgeKey(p1)];
                var nexus = new PreNexus(key, point, 
                    p1, p2, p3, 
                    e1, e2, e3);
                e1.SetNexus(nexus);
                e2.SetNexus(nexus);
                e3.SetNexus(nexus);
                res.Add(nexus);
            }
            else if(incidentPolys.Count != 1)
            {
                throw new Exception(incidentPolys.Count.ToString());
            }
        }

        return res;
    }
    
    
    // private static Dictionary<Vector3I, PreNexus> MakeNexiOld(List<PreCell> cells, 
    //     Vector2I dim, GenWriteKey key)
    // {
    //     var res = new Dictionary<Vector3I, PreNexus>();
    //     foreach (var cell1 in cells)
    //     {
    //         foreach (var cell2 in cell1.Neighbors)
    //         {
    //             if (cell2.PrePoly.Id >= cell1.PrePoly.Id) continue;
    //             foreach (var cell3 in cell2.Neighbors)
    //             {
    //                 if (cell3.PrePoly.Id >= cell1.PrePoly.Id) continue;
    //                 if (cell3.PrePoly.Id >= cell2.PrePoly.Id) continue;
    //                 if (cell3.Neighbors.Contains(cell1) == false) continue;
    //                 var (p11, p12) = cell1.EdgeWith(cell2);
    //                 var (p21, p22) = cell1.EdgeWith(cell3);
    //                 Vector2 pointRel;
    //                 if (p11 == p21) pointRel = p11;
    //                 else if (p11 == p22) pointRel = p11;
    //                 else if (p12 == p21) pointRel = p12;
    //                 else if (p12 == p22) pointRel = p12;
    //                 else throw new Exception();
    //                 var pointAbs = (cell1.RelTo + pointRel).ClampPosition(key.Data);
    //
    //                 var idKey = new Vector3I(cell1.PrePoly.Id, cell2.PrePoly.Id, cell3.PrePoly.Id);
    //                 if (res.ContainsKey(idKey))
    //                 {
    //                     throw new Exception("duplicate nexus at " + pointAbs);
    //                 }
    //                 res.Add(idKey,
    //                     new PreNexus(key, pointAbs, cell1.PrePoly, cell2.PrePoly, cell3.PrePoly));
    //             }
    //         }
    //     }
    //
    //     return res;
    // }
}