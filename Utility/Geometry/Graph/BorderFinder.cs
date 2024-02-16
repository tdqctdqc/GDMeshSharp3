
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BorderFinder
{
    // public static List<List<T>> FindBordersLeftToRight<T>(HashSet<T> set,
    //     Func<T, IEnumerable<T>> getNeighbors,
    //     Func<T, T, Vector2> getOffset,
    //     Func<T, T, bool> goodBorder)
    // {
    //     var boundaryElements = set
    //         .Where(t => getNeighbors(t).Any(n => set.Contains(n) == false && goodBorder(t, n)));
    //     var boundaryElUnions = UnionFind.Find(boundaryElements,
    //         (t, r) => true, getNeighbors);
    //
    //     var res = new List<List<T>>();
    //     foreach (var boundaryElUnion in boundaryElUnions)
    //     {
    //         
    //     }
    // }
}