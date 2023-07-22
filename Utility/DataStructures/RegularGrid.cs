using Godot;
using System;
using System.Collections.Generic;

public class RegularGrid<T>
{
    public Dictionary<Vector2, List<T>> Cells;
    private Dictionary<T, Vector2> _coords;
    private Func<T, Vector2> _posFunc;
    public float PartitionLength;
    public RegularGrid(Func<T, Vector2> posFunc, float partitionLength)
    {
        _posFunc = posFunc;
        PartitionLength = partitionLength;
        Cells = new Dictionary<Vector2, List<T>>();
        _coords = new Dictionary<T, Vector2>();
    }
    public void AddElement(T element, int x, int y)
    {
        var key = new Vector2(x,y);
        if(Cells.ContainsKey(key) == false)
        {
            Cells.Add(key, new List<T>());
        }
        Cells[key].Add(element);
        _coords.Add(element, key);
    }
    public void AddElement(T element)
    {
        var pos = _posFunc(element);
        var key = new Vector2((int)(pos.X / PartitionLength),(int)(pos.Y / PartitionLength));
        if(Cells.ContainsKey(key) == false)
        {
            Cells.Add(key, new List<T>());
        }
        Cells[key].Add(element);
    }

    public void Update()
    {
        foreach (var element in _coords.Keys)
        {
            UpdateElement(element, _posFunc(element));
        }
    }
    public void UpdateElement(T element, Vector2 newPos)
    {
        var newKey = new Vector2((int)(newPos.X / PartitionLength),(int)(newPos.Y / PartitionLength));
        var oldKey = _coords[element];
        if (_coords[element] == newKey)
            return;
        Cells[oldKey].Remove(element);
        if(Cells.ContainsKey(newKey) == false)
        {
            Cells.Add(newKey, new List<T>());
        }

        _coords[element] = newKey;
        Cells[newKey].Add(element);
    }
    public List<T> GetElementsAtPoint(Vector2 point)
    {
        int x = (int)(point.X / PartitionLength);
        int y = (int)(point.Y / PartitionLength);
        var key = new Vector2(x,y);

        if(Cells.ContainsKey(key)) return Cells[key];
        return new List<T>();
    }
    
    public List<T> GetElementsInRadius(Vector2 point, float radius)
    {
        int radiusIncrements = Mathf.CeilToInt(radius / PartitionLength);
        int x = (int)(point.X / PartitionLength);
        int y = (int)(point.Y / PartitionLength);
        var result = new List<T>();
        for (int i = x - radiusIncrements; i < x + radiusIncrements; i++)
        {
            for (int j = y - radiusIncrements; j < y + radiusIncrements; j++)
            {
                var key = new Vector2(i,j);

                if (Cells.ContainsKey(key))
                {
                    var cell = Cells[key];
                    foreach (var t in cell)
                    {
                        if (_posFunc(t).DistanceTo(point) < radius)
                        {
                            result.Add(t);
                        }
                    }
                }
            }
        }
        return result;
    }
    public void Clear()
    {
        Cells.Clear();
    }
}