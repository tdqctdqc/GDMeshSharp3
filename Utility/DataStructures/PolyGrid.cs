
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyGrid<TPoly>
    where TPoly : class
{
    public Dictionary<Vector2I, HashSet<TPoly>> Cells { get; private set; }
    public (Vector2I, float)[] SearchSpiralKeyOffsets { get; private set; }
    public float CellWidth { get; }
    public float CellHeight { get; }
    public float MaxCellDim => Mathf.Max(CellWidth, CellHeight);
    public int NumXPartitions { get; }
    public int NumYPartitions { get; }
    public Vector2 Dimension { get; }
    private Func<TPoly, Vector2[]> _getRelBoundary;
    private Func<TPoly, Vector2> _getCenter;
    
    public PolyGrid(Vector2 dim, float maxCellSideLength,
        Func<TPoly, Vector2[]> getRelBoundary, 
        Func<TPoly, Vector2> getCenter)
    {
        _getRelBoundary = getRelBoundary;
        _getCenter = getCenter;
        NumXPartitions = Mathf.CeilToInt(dim.X / maxCellSideLength);
        CellWidth = dim.X / NumXPartitions;
        NumYPartitions = Mathf.CeilToInt(dim.Y / maxCellSideLength);
        CellHeight = dim.Y / NumYPartitions;
        Cells = new Dictionary<Vector2I, HashSet<TPoly>>();
        Dimension = dim;
        for (var i = 0; i < NumXPartitions; i++)
        {
            for (var j = 0; j < NumYPartitions; j++)
            {
                var key = new Vector2I(i, j);
                Cells.Add(key, new HashSet<TPoly>());
            }
        }

        SearchSpiralKeyOffsets = GetSearchSpiral();
    }
    public void AddElement(TPoly poly)
    {
        var boundary = _getRelBoundary(poly);
        var center = _getCenter(poly);
        if (boundary == null) return;
        int minXCell = int.MaxValue;
        int maxXCell = int.MinValue;
        int minYCell = int.MaxValue;
        int maxYCell = int.MinValue;
        foreach (var relP in boundary)
        {
            var absP = center + relP;
            var key = GetUnclampedKey(absP);
            minXCell = Mathf.Min(key.X, minXCell);
            maxXCell = Mathf.Max(key.X, maxXCell);
            minYCell = Mathf.Min(key.Y, minYCell);
            maxYCell = Mathf.Max(key.Y, maxYCell);
        }

        for (int i = minXCell; i <= maxXCell; i++)
        {
            for (int j = minYCell; j <= maxYCell; j++)
            {
                var key = ClampKey(new Vector2I(i, j));
                if (Cells.ContainsKey(key) == false)
                {
                    Cells.Add(key, new HashSet<TPoly>());
                }
                Cells[key].Add(poly);
            }
        }
    }
    private Vector2I GetUnclampedKey(Vector2 point)
    {
        int x = Mathf.FloorToInt(point.X / CellWidth);
        int y = Mathf.FloorToInt(point.Y / CellHeight);
        if (y == NumYPartitions) y--;
        return new Vector2I(x,y);
    }

    public bool CheckIfElementAtPoint()
    {
        return true;
    }
    public TPoly GetElementAtPoint(Vector2 point, Data d)
    {
        var key = ClampKey(GetKey(point));
        if (Cells.ContainsKey(key) == false)
        {
            throw new Exception($"no key {key}, x partitions {NumXPartitions} y partitions {NumYPartitions}");
        }
        var found = Cells[key]
            .FirstOrDefault(p => PointInPolyAbs(p, point, d));
        if (found != null) return found;  
        
        var (poly, issue) = ForceGet(point, key, d);
        d.ClientPlayerData.Issues.Add(issue);
        return poly;
    }

    private bool PointInPolyAbs(TPoly poly, Vector2 p, Data d)
    {
        var boundary = _getRelBoundary(poly);
        var center = _getCenter(poly);
        var posRel = center.Offset(p, d);
        return Geometry2D.IsPointInPolygon(posRel, boundary);
    }
    private (TPoly, NoPolyAtPointIssue<TPoly>) ForceGet(Vector2 point, Vector2I key, Data d)
    {
        var issue = new NoPolyAtPointIssue<TPoly>(point, null, "");
        issue.Message += $"couldn't find poly at point {point}" +
                         $" cell {key}, force getting";
        var (close, dist) = GetClosestInCell(point, key, d);
        if (close != null)
        {            
            issue.Message += $"\nfound close poly for {point} at dist {dist}";
            issue.FoundPoly = close;
            return (close, issue);
        }
        GD.Print("no close poly in cell, count is " + Cells[key].Count);
        issue.Message += $"\ncouldn't find close poly at point {point}" +
               $"cell {key}, expanding search";

        (TPoly, float) res = (null, Mathf.Inf);
        for (int i = -1; i < 1; i++)
        {
            for (int j = -1; j < 1; j++)
            {
                if (i == 0 && j == 0) continue;
                if (j < 0 || j >= NumYPartitions) continue;
                var nKey = new Vector2I(i, j);
                nKey = ClampKey(nKey);
                
                
                var nRes = GetClosestInCell(point, nKey, d);
                if (nRes.Item1 != null && nRes.Item2 < res.Item2)
                {
                    res = nRes;
                }
            }
        }

        if (res.Item1 != null)
        {
            issue.Message += $"\nfound close neighbor cell poly for {point} at dist {res.Item2}";
            issue.FoundPoly = res.Item1;
            return (res.Item1, issue);
        }
        throw new Exception($"finally couldn't find any close polys at ${point}" +
                            $"key {key}");
    }

    private (TPoly, float) GetClosestInCell(Vector2 point, Vector2I key, Data d)
    {
        if (Cells.ContainsKey(key) == false)
        {
            return (null, Mathf.Inf);
        }
        var cell = Cells[key];
        if (cell.Count == 0)
        {
            var issue = new NoPolysInCellIssue(key, point, "");
            d.ClientPlayerData.Issues.Add(issue);
            return (null, Mathf.Inf);
        }

        var contains = cell.FirstOrDefault(p => 
            PointInPolyAbs(p, point, d));
        if (contains != null) return (contains, 0f);
        
        
        var dist = Mathf.Inf;
        TPoly res = null;
        foreach (var p in cell)
        {
            var center = _getCenter(p);
            var rel = center.Offset(point, d);
            var bps = _getRelBoundary(p);
            var minDist = Mathf.Inf;
            for (var i = 0; i <= bps.Length; i++)
            {
                var from = bps.Modulo(i);
                var to = bps.Modulo(i + 1);
                var closest = rel
                    .GetClosestPointOnLineSegment(from, to);
                minDist = Mathf.Min(minDist, rel.DistanceTo(closest));
            }

            if (minDist < dist)
            {
                dist = Mathf.Min(dist, minDist);
                res = p;
            }
        }
        return (res, dist);
    }
    private Vector2I ClampKey(Vector2I key)
    {
        key.X = GetXModulo(key.X);
        key.Y = Mathf.Clamp(key.Y, 0, NumYPartitions - 1);
        return key;
    }
    private Vector2I GetKey(Vector2 point)
    {
        int x = Mathf.FloorToInt(point.X / CellWidth);
        x = GetXModulo(x);
        int y = Mathf.FloorToInt(point.Y / CellHeight);
        if (y == NumYPartitions) y--;
        return new Vector2I(x,y);
    }
    private int GetXModulo(int x)
    {
        while (x < 0) x += NumXPartitions;
        while (x > NumXPartitions - 1) x -= NumXPartitions;
        return x;
    }
    private (Vector2I, float)[] GetSearchSpiral()
    {
        var xRange = Mathf.CeilToInt(NumXPartitions / 2) + 1;
        var yRange = Mathf.CeilToInt(NumYPartitions / 2) + 1;
        var offsets = new List<(Vector2I, float)>();
        
        for (var i = -xRange; i <= xRange; i++)
        {
            for (var j = -yRange; j <= yRange; j++)
            {
                var pos = new Vector2(i * CellWidth, j * CellHeight);
                var key = new Vector2I(i, j);
                offsets.Add((key, pos.Length()));
            }
        }

        return offsets.OrderBy(v => v.Item2).ToArray();
    }
}