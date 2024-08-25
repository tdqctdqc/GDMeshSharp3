using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AdjacencyCountPickerAgent<TItem> 
    : PickerAgent<TItem>
{
    public AdjacencyCountPickerAgent(TItem seed, 
        Picker<TItem> host, int numToPick, Func<TItem, bool> valid,
        Data data) 
        : base(seed, host, numToPick, valid, data)
    {
    }
    
    public override bool Pick(Picker<TItem> host, Data data)
    {
        if (ValidAdjacent.Any(host.NotTaken.Contains) == false) return false;

        if (Picked.Count < 4)
        {
            foreach (var a in ValidAdjacent)
            {
                if (host.NotTaken.Contains(a) == false) continue;
                Add(a, host, data);
                return true;
            }
            return false;
        }

        var aCount = 0;
        TItem pick = default;
        var found = false;
        
        foreach (var a in ValidAdjacent.ToArray())
        {
            if (host.NotTaken.Contains(a) == false)
            {
                ValidAdjacent.Remove(a);
                continue;
            }
            var count = host.GetNeighbors(a)
                .Count(n => Picked.Contains(n));
            if (count <= 1) continue;

            if (count > aCount)
            {
                pick = a;
                aCount = count;
                found = true;
            }
        }
        if (found)
        {
            Add(pick, host, data);
            return true;
        }

        return false;
    }
}
