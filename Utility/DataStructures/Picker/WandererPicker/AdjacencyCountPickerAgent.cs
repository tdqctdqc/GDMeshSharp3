using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AdjacencyCountPickerAgent<T> : IPickerAgent<T>
{
    public HashSet<T> Seeds { get; private set; }
    public HashSet<T> Picked { get; private set; }
    public Dictionary<T, int> Adjacents { get; private set; }
    private Func<T, bool> _valid;
    public int NumToPick { get; private set; }

    public AdjacencyCountPickerAgent(T seed, Picker<T> host, int numToPick, 
        Func<T, bool> valid)
    {
        Seeds = new HashSet<T> { seed };
        _valid = valid;
        NumToPick = numToPick;
        Picked = new HashSet<T>();
        Adjacents = new Dictionary<T, int>();
        host.AddAgent(this);
        Add(seed, host);
    }

    public bool Pick(Picker<T> host)
    {
        while (true)
        {
            var avail = Adjacents.Keys.Intersect(host.NotTaken);
            if (avail.Any() == false) return false;
            var max = avail.MaxBy(a => Adjacents[a]);
            if (_valid(max))
            {
                Add(max, host);
                return true;
            }
            else
            {
                Adjacents.Remove(max);
            }
        }
    }

    

    protected void Add(T t, Picker<T> host)
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
