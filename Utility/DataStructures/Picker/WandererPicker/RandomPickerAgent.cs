
using System;
using System.Collections.Generic;
using System.Linq;

public class RandomPickerAgent<T> : IPickerAgent<T>
{
    public HashSet<T> Seeds { get; private set; }
    public HashSet<T> Picked { get; private set; }
    public HashSet<T> Adjacents { get; private set; }
    private Func<T, bool> _valid;
    public int NumToPick { get; private set; }

    public RandomPickerAgent(T seed, Picker<T> host, int numToPick, 
        Func<T, bool> valid)
    {
        Seeds = new HashSet<T> { seed };
        _valid = valid;
        NumToPick = numToPick;
        Picked = new HashSet<T>();
        Adjacents = new HashSet<T>();
        host.AddAgent(this);
        Add(seed, host);
    }

    public bool Pick(Picker<T> host)
    {
        while (true)
        {
            var avail = Adjacents
                .Intersect(host.NotTaken);
            if (avail.Any() == false) return false;

            var max = avail.GetRandomElement();
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
                Adjacents.Add(n);
            }
        }
    }

}