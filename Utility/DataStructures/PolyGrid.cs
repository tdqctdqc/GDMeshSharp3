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
            for (int j = minYCoord; j <= maxYCoord; j++)
            {
                var key = new Vector2(i, j);
                Cells.AddOrUpdate(key, element);
            }
        }
    }

    public void Update()
    {
        Cells = new Dictionary<Vector2, List<MapPolygon>>();
        foreach (var element in _data.GetAll<MapPolygon>())
        {
            AddElement(element);
        }
    }

    private MapPolygon ForceGetClosestAtPoint(Vector2 point, Data d)
    {
        int x = (int)(point.X / _partitionSize.X);
        int y = (int)(point.Y / _partitionSize.Y);
        var key = new Vector2(x,y);
        List<MapPolygon> polys = null;
        try
        {
            polys = Cells[key];
        }
        catch (Exception e)
        {
            GD.Print($"couldn't find polys at {point} cell {key}");
            GD.Print($"{_partitionsPerAxis} partitions per axis");
        }
        return polys.MinBy(p =>
        {
            var rel = p.Center.GetOffsetTo(point, d);
            var bps = p.GetOrderedBoundaryPoints(d);
            var minDist = Mathf.Inf;
            for (var i = 0; i < bps.Length; i++)
            {
                var from = bps.Modulo(i);
                var to = bps.Modulo(i + 1);
                var closest = rel
                    .GetClosestPointOnLineSegment(from, to);
                return rel.DistanceTo(closest);
            }

            return minDist;
        });
        

    }
    public MapPolygon GetElementAtPoint(Vector2 point, Data d)
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

        if (found == null)
        {
            return ForceGetClosestAtPoint(point, d);
        }
        return found;
    }


    private bool CheckCell(Vector2 point, Vector2 key, out MapPolygon p)
    {
        if (Cells.TryGetValue(key, out var cell))
        {
            p = cell.FirstOrDefault(mp => 
                mp.PointInPolyAbs(point, _data));
            if (p != null)
            {
                return true;
            }
        }

        p = null;
        return false;
    }
}