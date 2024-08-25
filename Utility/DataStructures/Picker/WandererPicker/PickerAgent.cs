using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class PickerAgent<T>
{
    public HashSet<T> Picked { get; private set; }
    public HashSet<T> ValidAdjacent { get; private set; }
    private Func<T, bool> _valid;
    public int NumToPick { get; private set; }

    public PickerAgent(T seed, Picker<T> host, int numToPick, 
        Func<T, bool> valid,
        Data data)
    {
        _valid = valid;
        NumToPick = numToPick;
        Picked = new HashSet<T>();
        ValidAdjacent = new HashSet<T>();
        host.AddAgent(this);
        Add(seed, host, data);
    }

    public abstract bool Pick(Picker<T> host, Data data);

    protected void Add(T t, Picker<T> host, Data data)
    {
        Picked.Add(t);
        host.NotTaken.Remove(t);
        ValidAdjacent.Remove(t);
        
        var outside = host.GetNeighbors(t)
            .Where(p => Valid(p) 
                        && host.NotTaken.Contains(p)
                        && Picked.Contains(p) == false)
            .Except(Picked);
        ValidAdjacent.UnionWith(outside);
    }

    protected bool Valid(T poly)
    {
        return _valid(poly);
    }
}
