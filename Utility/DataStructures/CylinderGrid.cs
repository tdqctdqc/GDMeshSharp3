
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CylinderGrid<T> 
{
    public Dictionary<Vector2I, HashSet<T>> Cells { get; private set; }
    public (Vector2I, float)[] SearchSpiralKeyOffsets { get; private set; }
    public float CellWidth { get; }
    public float CellHeight { get; }
    public float MaxCellDim => Mathf.Max(CellWidth, CellHeight);
    public int NumXPartitions { get; }
    public int NumYPartitions { get; }
    public Vector2 Dimension { get; }
    private Func<T, Vector2> _getPos;
    public CylinderGrid(Vector2 dim, float maxCellSideLength,
        Func<T, Vector2> getPos)
    {
        _getPos = getPos;
        NumXPartitions = Mathf.CeilToInt(dim.X / maxCellSideLength);
        CellWidth = dim.X / NumXPartitions;
        NumYPartitions = Mathf.CeilToInt(dim.Y / maxCellSideLength);
        CellHeight = dim.Y / NumYPartitions;
        Cells = new Dictionary<Vector2I, HashSet<T>>();
        Dimension = dim;
        for (var i = 0; i < NumXPartitions; i++)
        {
            for (var j = 0; j < NumYPartitions; j++)
            {
                var key = new Vector2I(i, j);
                Cells.Add(key, new HashSet<T>());
            }
        }

        SearchSpiralKeyOffsets = GetSearchSpiral();
    }
    
    public bool TryGetClosest(Vector2 p, out T close)
    {
        close = default;
        var startKey = GetKey(p);
        bool found = false;
        float dist = Mathf.Inf;
        for (var i = 0; i < SearchSpiralKeyOffsets.Length; i++)
        {
            var v = SearchSpiralKeyOffsets[i];
            var keyOffset = v.Item1;
            var keyDist = v.Item2;
            if (found && keyDist > dist + MaxCellDim)
            {
                break;
            }
            var key = startKey + keyOffset;
            key = ClampKey(key);
            var set = Cells[key];
            if (set.Count > 0)
            {
                var closeInCell = set.MinBy(t => GetDist(_getPos(t), p));
                var closeInCellDist = GetDist(_getPos(closeInCell), p);
                if (closeInCellDist < dist)
                {
                    found = true;
                    dist = closeInCellDist;
                    close = closeInCell;
                }
            }
        }
        return found;
    }

    private float GetDist(Vector2 p1, Vector2 p2)
    {
        var yDist = Mathf.Abs(p1.Y - p2.Y);
        var xDist = Mathf.Abs(p1.X - p2.X);
        if (xDist > Dimension.X / 2f) xDist = Dimension.X - xDist;
        return Mathf.Sqrt(yDist * yDist + xDist * xDist);
    }
    private int GetXModulo(int x)
    {
        while (x < 0) x += NumXPartitions;
        while (x > NumXPartitions - 1) x -= NumXPartitions;
        return x;
    }

    public void Add(T t)
    {
        var key = GetKey(_getPos(t));
        if (Cells.ContainsKey(key) == false)
        {
            throw new Exception($"no key {key} partitions are {new Vector2(NumXPartitions, NumYPartitions)}");
        }
        Cells[key].Add(t);
    }

    private Vector2I ClampKey(Vector2I key)
    {
        while (key.X >= NumXPartitions) key.X -= NumXPartitions;
        while (key.X < 0) key.X += NumXPartitions;
        while (key.Y >= NumYPartitions) key.Y -= NumYPartitions;
        while (key.Y < 0) key.Y += NumYPartitions;
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