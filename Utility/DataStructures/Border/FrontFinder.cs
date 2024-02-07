
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class FrontFinder
{
    public static List<List<FrontFace>> FindFront(
        HashSet<PolyCell> natives, Data d)
    {
        var res = new List<List<FrontFace>>();
        var native = natives.First();
        bool isForeign(PolyCell c)
        {
            return c.Controller.RefId != -1
                && c.Controller.RefId != native.Controller.RefId;
        }
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
            var front = first.GetFrontLeftToRight(oppositionsHash.Contains, d);
            oppositionsHash.ExceptWith(front);
            res.Add(front);
        }
        return res;
    }
    
    
    
}