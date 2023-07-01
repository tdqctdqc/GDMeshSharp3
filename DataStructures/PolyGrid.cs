using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyGrid
{
    public Dictionary<Vector2, List<MapPolygon>> Cells;
    private Vector2 _partitionSize;
    private int _partitionsPerAxis;
    private Data _data;
    
    public PolyGrid(int numPartitionsPerAxis, Vector2 dim, Data data)
    {
        _partitionsPerAxis = numPartitionsPerAxis;
        _data = data;
        _partitionSize = dim / numPartitionsPerAxis;
        Cells = new Dictionary<Vector2, List<MapPolygon>>();
    }
    public void AddElement(MapPolygon element)
    {
        var boundaryPoints = element.GetOrderedBoundaryPoints(_data);
        if (boundaryPoints == null || boundaryPoints.Length == 0) return;
        var minX = boundaryPoints.Min(p => p.X);
        var minXCoord = Mathf.FloorToInt(minX / _partitionSize.X);
        var maxX = boundaryPoints.Max(p => p.X);
        var maxXCoord = Mathf.CeilToInt(maxX / _partitionSize.X);

        var minY = boundaryPoints.Min(p => p.Y);
        var minYCoord = Mathf.FloorToInt(minY / _partitionSize.Y);

        var maxY = boundaryPoints.Max(p => p.Y);
        var maxYCoord = Mathf.CeilToInt(maxY / _partitionSize.Y);

        for (int i = minXCoord; i <= maxXCoord; i++)
        {
            for (int j = minYCoord; j < maxYCoord; j++)
            {
                var key = new Vector2(i, j);
                if(Cells.ContainsKey(key) == false)
                {
                    Cells.Add(key, new List<MapPolygon>());
                }
                Cells[key].Add(element);
            }
        }
        
    }

    public void Update()
    {
        Cells = new Dictionary<Vector2, List<MapPolygon>>();
        foreach (var element in _data.Planet.Polygons.Entities)
        {
            AddElement(element);
        }
    }
    public MapPolygon GetElementAtPoint(Vector2 point)
    {
        int x = (int)(point.X / _partitionSize.X);
        int y = (int)(point.Y / _partitionSize.Y);
        var key = new Vector2(x,y);
        if (CheckCell(point, key, out var mp1))
        {
            return mp1;
        }
        if (x == _partitionsPerAxis - 1)
        {
            var offKey = new Vector2(0, y);
            if (CheckCell(point, offKey, out var mp2))
            {
                return mp2;
            }
        }

        MapPolygon found = null;
        EnumerableExt.DoForGridAround(
            (int i, int j) =>
            {
                var offKey = new Vector2(i, j);
                if (CheckCell(point, offKey, out var p))
                {
                    found = p;
                    return false;
                }
                return true;
            }, x, y
        );
        return found;
    }


    private bool CheckCell(Vector2 point, Vector2 key, out MapPolygon p)
    {
        if (Cells.TryGetValue(key, out var cell))
        {
            p = cell.FirstOrDefault(mp => mp.PointInPolyAbs(point, _data));
            if (p != null)
            {
                return true;
            }
        }

        p = null;
        return false;
    }
}