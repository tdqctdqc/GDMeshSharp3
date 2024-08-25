using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AdjacencyCountPickerAgent<T> : IPickerAgent<T>
{
    public HashSet<T> Picked { get; private set; }
    public Dictionary<T, int> Adjacents { get; private set; }
    private Func<T, bool> _valid;
    public int NumToPick { get; private set; }

    public AdjacencyCountPickerAgent(T seed, Picker<T> host, int numToPick, 
        Func<T, bool> valid,
        Data data)
    {
        _valid = valid;
        NumToPick = numToPick;
        Picked = new HashSet<T>();
        Adjacents = new Dictionary<T, int>();
        host.AddAgent(this);
        Add(seed, host, data);
    }

    public bool Pick(Picker<T> host, Data data)
    {
        while (true)
        {
            if (Adjacents.Count == 0) return false;
            var max = Adjacents.MaxBy(kvp => kvp.Value).Key;
            if (host.NotTaken.Contains(max))
            {
                Add(max, host, data);
                return true;
            }
            else
            {
                Adjacents.Remove(max);
            }
        }
    }

    protected void Add(T t, Picker<T> host, Data data)
    {
        Picked.Add(t);
        host.NotTaken.Remove(t);
        Adjacents.Remove(t);
        
        foreach (var n in host.GetNeighbors(t))
        {
            if (_valid(n) && host.NotTaken.Contains(n))
            {
                Adjacents.AddOrSum(n, 1);
            }
        }
    }

}
