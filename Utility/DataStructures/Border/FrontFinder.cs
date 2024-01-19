
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class FrontFinder
{
    public static List<Vector2I> FindFrontSimple<T>(
        IEnumerable<T> elements,
        Func<T, IEnumerable<T>> getNeighbors,
        Func<T, T, Vector2> getOffset,
        Func<T, int> getId)
            where T : class
    {
        var links = new List<Vector2I>();
        var elHash = new HashSet<T>(elements);
        var linkHash = new HashSet<Vector2I>();

        int iter = 0;
        int maxIter = 10_000;
        var from = elements.First();
        var to = getNeighbors(from)
            .Where(elHash.Contains).First();
        while (true)
        {
            iter++;
            if (iter > maxIter) throw new Exception();
            var link = new Vector2I(getId(from), getId(to));
            if (linkHash.Contains(link)) break;
            linkHash.Add(link);
            links.Add(link);
            var axis = getOffset(to, from);
            var next = getNeighbors(to)
                .Where(elHash.Contains)
                .MinBy(n => axis.GetCWAngleTo(getOffset(to, n)));
            from = to;
            to = next;
        }
        return links;
    }
    
    
    
    public static List<List<(T native, T foreign)>> FindFrontNew<T>(
        IEnumerable<T> elements,
        Func<T, bool> isForeign,
        Func<T, bool> isNative,
        Func<T, IEnumerable<T>> getNeighbors,
        Func<T, T, Vector2> getOffset)
        where T : class
    {
        
        var res = new List<List<(T native, T foreign)>>();
        var oppositions = new HashSet<(T native, T foreign)>();
        oppositions.AddRange(
            elements.SelectMany(e =>
            {
                return getNeighbors(e)
                    .Where(isForeign)
                    .Select(f => (e, f));
            })
        );

        int maxIter = 10_000;
        while (oppositions.Count > 0)
        {
            var first = oppositions.First();
            oppositions.Remove(first);
            var list = new LinkedList<(T native, T foreign)>();
            list.AddFirst(first);
            go(first, t => list.AddFirst(t));
            go(first, t => list.AddLast(t));
            res.Add(list.ToList());
        }
        return res;

        void go((T native, T foreign) edge,
            Action<(T native, T foreign)> add)
        {
            var mutualNs = getNeighbors(edge.native)
                .Intersect(getNeighbors(edge.foreign))
                .Where(n => isNative(n) || isForeign(n))
                .Select(n =>
                {
                    if (isForeign(n))
                    {
                        return (edge.native, n);
                    }

                    return (n, edge.foreign);
                })
                .Where(oppositions.Contains);
            if (mutualNs.Count() == 0) return;
            
            var e = mutualNs.First();
            add(e);
            oppositions.Remove(e);
            go(e, add);
        }
    }

    public static List<List<(T native, T foreign)>> FindFront<T>(
        IEnumerable<T> elements,
        Func<T, bool> foreign,
        Func<T, IEnumerable<T>> getNeighbors,
        Func<T, T, Vector2> getOffset)
            where T : class
    {
        var res = new List<List<(T native, T foreign)>>();

        var oppositions = new HashSet<(T native, T foreign)>();
        oppositions.AddRange(
            elements.SelectMany(e =>
            {
                return getNeighbors(e).Where(foreign)
                    .Select(f => (e, f));
            })
        );

        while (oppositions.Count > 0)
        {
            var start = oppositions.First();
            oppositions.Remove(start);
            var list = new LinkedList<(T native, T foreign)>();
            list.AddFirst(start);

            var curr = start;
            while (true)
            {
                var next = getNext(curr);
                if (next.native == null) break;
                list.AddLast(next);
                curr = next;
            }

            curr = start;
            while (true)
            {
                var next = getNext(curr);
                if (next.native == null) break;
                list.AddFirst(next);
                curr = next;
            }
            res.Add(list.ToList());
        }

        return res;
        (T native, T foreign) getNext((T native, T foreign) curr)
        {
            var currForeignClose =
                getNeighbors(curr.native).Where(foreign)
                    .Select(f => (curr.native, f))
                    .Where(f => oppositions.Contains(f) 
                                && f.f != curr.foreign
                                && getNeighbors(curr.foreign).Contains(f.f));
            if (currForeignClose.Count() > 0)
            {
                var offset = getOffset(curr.native, curr.foreign);
                var next = currForeignClose
                    .OrderBy(f =>
                    {
                        var fOffset = getOffset(f.native, f.f);
                        return offset.AngleTo(fOffset);
                    })
                    .First();
                oppositions.Remove(next);
                return next;
            }
                
                
            var currForeign =
                getNeighbors(curr.native).Where(foreign)
                    .Select(f => (curr.native, f))
                    .Where(f => oppositions.Contains(f)
                                && f.f != curr.foreign);
            if (currForeign.Count() > 0)
            {
                var offset = getOffset(curr.native, curr.foreign);
                var next = currForeign.OrderBy(f =>
                {
                    var fOffset = getOffset(f.native, f.f);
                    return offset.AngleTo(fOffset);
                }).First();
                oppositions.Remove(next);
                return next;
            }
                
                
                
            var foreignNative = getNeighbors(curr.foreign)
                .Where(n => foreign(n) == false && getNeighbors(curr.native).Contains(n))
                .Select(n => (n, curr.foreign))
                .Where(f => oppositions.Contains(f) 
                    && f.n != curr.native);
            if (foreignNative.Count() > 0)
            {
                var next = foreignNative.First();
                oppositions.Remove(next);
                return next;
            }

            return (null, null);
        }
    }
}