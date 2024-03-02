
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class FrontFinder
{
    public static List<List<FrontFace>> FindFront(
        HashSet<Cell> natives, 
        Func<Cell, bool> isForeign,
        Data d)
    {
        var res = new List<List<FrontFace>>();
        var oppositions = natives.SelectMany(e =>
        {
            return e.GetNeighbors(d)
                .Where(isForeign)
                .Select(f => FrontFace.Construct(e, f, d));
        }).ToHashSet();
        var oppositionsHash = oppositions.ToHashSet();
        while (oppositionsHash.Count > 0)
        {
            var first = oppositionsHash.First();
            var front = first.GetFrontLeftToRight(
                natives.Contains,
                oppositionsHash.Contains, d);
            oppositionsHash.ExceptWith(front);
            res.Add(front);
        }
        return res;
    }
    
    
    
}