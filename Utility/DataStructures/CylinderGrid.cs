
using System;
using System.Collections.Generic;
using Godot;

public class CylinderGrid<T> 
{
    public Dictionary<Vector2I, HashSet<T>> Cells { get; private set; }
    public float CellWidth { get; }
    public float CellHeight { get; }
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
    }
    
    public bool TryGetClosest(Vector2 p, out T close)
    {
        var startKey = GetKey(p);
        T closeInner = default;
        bool found = false;
        var dist = Mathf.Inf;
        void pick(T t)
        {
            var pos = _getPos(t);
            var tDist = GetDist(pos, p);
            if (tDist < dist)
            {
                dist = tDist;
                closeInner = t;
                found = true;
            }
        }
        SearchRingAroundForAll(startKey, pick);
        close = closeInner;
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

    private void SearchRingAroundForAll(Vector2I centerKey, Action<T> action)
    {
        var xToSearch = Mathf.CeilToInt(NumXPartitions / 2f);
        var yToSearch = Mathf.CeilToInt(NumYPartitions / 2f);
        var toSearch = Mathf.Max(xToSearch, yToSearch);
        for (var i = 0; i < toSearch; i++)
        {
            SearchRing(centerKey, i, action);
        }
    }
    private void SearchRingAroundForFirst(Vector2I centerKey, Action<T> action)
    {
        var xToSearch = Mathf.CeilToInt(NumXPartitions / 2f);
        var yToSearch = Mathf.CeilToInt(NumYPartitions / 2f);
        var toSearch = Mathf.Max(xToSearch, yToSearch);
        bool found = false;
        Action<T> action2 = t =>
        {
            action(t);
            found = true;
        };
        for (var i = 0; i < toSearch; i++)
        {
            SearchRing(centerKey, i, action);
            if (found) break;
        }
    }
    private void SearchRing(Vector2I centerKey, int dist,
        Action<T> action)
    {
        var minX = centerKey.X - dist;
        var maxX = centerKey.X + dist;
        var minY = centerKey.Y - dist;
        var maxY = centerKey.Y + dist;
        for (int i = minX; i <= maxX; i++)
        {
            var xIter = GetXModulo(i);
            if (minY >= 0)
            {
                foreach (var t in Cells[new Vector2I(xIter, minY)])
                {
                    action(t);
                }
            }

            if (maxY < NumYPartitions)
            {
                foreach (var t in Cells[new Vector2I(xIter, maxY)])
                {
                    action(t);
                }
            }
        }
        
        var minXModulo = GetXModulo(minX);
        var maxXModulo = GetXModulo(maxX);
        for (int i = minY; i <= maxY; i++)
        {
            if (i >= 0 && i < NumYPartitions)
            {
                foreach (var t in Cells[new Vector2I(minXModulo, i)])
                {
                    action(t);
                }
                foreach (var t in Cells[new Vector2I(maxXModulo, i)])
                {
                    action(t);
                }
            }
        }
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
    private Vector2I GetKey(Vector2 point)
    {
        int x = Mathf.FloorToInt(point.X / CellWidth);
        x = GetXModulo(x);
        int y = Mathf.FloorToInt(point.Y / CellHeight);
        if (y == NumYPartitions) y--;
        return new Vector2I(x,y);
    }
}