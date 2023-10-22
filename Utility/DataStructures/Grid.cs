
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Grid<T>
    where T : class
{
    public Dictionary<Vector2, List<T>> Cells;
    private Func<T, Vector2> _getPos;
    private Vector2 _partitionSize;
    private int _partitionsPerAxis;
    private Data _data;
    
    public Grid(int numPartitionsPerAxis, Vector2 dim, Func<T, Vector2> getPos,
        Data data)
    {
        _getPos = getPos;
        _partitionsPerAxis = numPartitionsPerAxis;
        _data = data;
        _partitionSize = dim / numPartitionsPerAxis;
        Cells = new Dictionary<Vector2, List<T>>();
    }
    public void AddElement(T element)
    {
        var pos = _getPos(element);
        var key = GetKey(pos);
        if(Cells.ContainsKey(key) == false)
        {
            Cells.Add(key, new List<T>());
        }
        Cells[key].Add(element);
    }

    private Vector2 GetKey(Vector2 point)
    {
        int x = (int)(point.X / _partitionSize.X);
        int y = (int)(point.Y / _partitionSize.Y);
        return new Vector2(x,y);
    }
    public T GetElementAtPoint(Vector2 point)
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

        T found = null;
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


    private bool CheckCell(Vector2 point, Vector2 key, out T p)
    {
        if (Cells.TryGetValue(key, out var cell))
        {
            p = cell.OrderBy(t => _getPos(t).DistanceTo(point)).FirstOrDefault();
            if (p != null)
            {
                return true;
            }
        }

        p = null;
        return false;
    }
}