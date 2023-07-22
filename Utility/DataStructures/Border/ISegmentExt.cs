using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class ISegmentExt
{
    public static List<List<TSeg>> ChainSort<TSeg, TPrim>(this IEnumerable<TSeg> segs) where TSeg: ISegment<TPrim>
    {
        var res = new List<List<TSeg>>();
        var count = segs.Count();
        var index = 0;
        res.Add(new List<TSeg> {segs.ElementAt(0)});
        var uf = UnionFind.Find(segs, (s, r) => s.PointsTo(r) || r.PointsTo(s), s => segs);
        return uf.Select(u => u.Chainify<TSeg, TPrim>()).ToList();
    }
    
    public static List<TSeg> Chainify<TSeg, TPrim>(this List<TSeg> segments)
        where TSeg : ISegment<TPrim>
    {
        var froms = new Dictionary<TPrim, TSeg>();
        var tos = new Dictionary<TPrim, TSeg>();
        TPrim first = default;
        bool foundFirst = false;
        for (var i = 0; i < segments.Count; i++)
        {
            var seg = segments[i];
            tos.Add(seg.To, seg);
        }
        for (var i = 0; i < segments.Count; i++)
        {
            var seg = segments[i];
            froms.Add(seg.From, seg);
            if (tos.ContainsKey(seg.From) == false)
            {
                foundFirst = true;
                first = seg.From;
            }
        }

        if (foundFirst == false) first = segments[0].From;

        var curr = froms[first];
        var res = new List<TSeg>{curr};
        
        for (var i = 0; i < segments.Count - 1; i++)
        {
            var next = froms[curr.To];
            res.Add(next);
            curr = next;
        }

        return res;
    }
    public static bool IsCircuit<TPrim>(this IReadOnlyList<ISegment<TPrim>> segs)
    {
        for (int i = 0; i < segs.Count - 1; i++)
        {
            if (segs[i].PointsTo(segs[i + 1]) == false) return false;
        }
        if (segs[segs.Count - 1].PointsTo(segs[0]) == false) return false;

        return true;
    }
    public static bool IsChain<TPrim>(this IReadOnlyList<ISegment<TPrim>> segs)
    {
        for (int i = 0; i < segs.Count - 1; i++)
        {
            if (segs[i].PointsTo(segs[i + 1]) == false) return false;
            if (segs[i].From.Equals(segs[i + 1].To)) return false;
        }

        return true;
    }
    public static TSeg Reverse<TSeg, TPrim>(this TSeg s)
        where TSeg : ISegment<TPrim>
    {
        return (TSeg)s.ReverseGeneric();
    }

    public static List<TSeg> Ordered<TSeg, TPrim>
        (this IEnumerable<TSeg> segs) where TSeg : ISegment<TPrim>
    {
        //todo make it sort existing list so dont need tseg
        var segCount = segs.Count();
        var segsSample = segs.ToList();
        var res = new List<TSeg>{segs.First()};
        segsSample.Remove(segs.First());
        //scan for next
        var currLast = res.Last();
        var next = segsSample.FirstOrDefault(s => s.ComesFrom(currLast));
        while (next != null && res.Count < segCount)
        {
            res.Add(next);
            segsSample.Remove(next);
            currLast = next;
            next = segsSample.FirstOrDefault(s => s.ComesFrom(currLast));
        }
        
        var currFirst = res[0];
        var prevRes = new List<TSeg>();
        var prev = segsSample.FirstOrDefault(s => s.PointsTo(currFirst));
        while (prev != null && prevRes.Count + res.Count < segCount)
        {
            prevRes.Add(prev);
            segsSample.Remove(prev);
            currFirst = prev;
            prev = segsSample.FirstOrDefault(s => s.PointsTo(currFirst));
        }

        prevRes.Reverse();
        prevRes.AddRange(res);
        
        return prevRes;
    }
    
    
    public static bool IsContinuous<TPrim>(this IReadOnlyList<ISegment<TPrim>> segs)
    {
        for (var i = 0; i < segs.Count - 1; i++)
        {
            if (segs[i].PointsTo(segs[i + 1]) == false) return false;
        }
        return true;
    }
    
}
