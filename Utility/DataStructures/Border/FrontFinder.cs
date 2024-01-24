
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


    public static List<List<(PolyCell native, PolyCell foreign)>>
        FindPolyCellFront(IEnumerable<PolyCell> cells,
            Alliance alliance,
            Func<PolyCell, bool> isNative,
            Data d)
    {
        return FindFront<PolyCell>(cells,
                c =>
                {
                    if (c.Controller.Empty()) return false;
                    var controllerRegime = c.Controller.Entity(d);
                    var controllerAlliance = controllerRegime.GetAlliance(d);
                    return alliance.Rivals.Contains(controllerAlliance);
                },
                isNative,
                c => c.GetNeighbors(d),
                (p,q) => p.GetCenter().GetOffsetTo(q.GetCenter(), d));
    }
    public static List<List<(T native, T foreign)>> FindFront<T>(
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
    public static List<(T native, T foreign)> 
        PathfindOnFront<T>((T native, T foreign) start,
            (T native, T foreign) dest,
            Func<T, IEnumerable<T>> getNeighbors,
            Func<T, bool> isForeign, 
            Func<T, bool> isNative,
            Func<T, T, Vector2> getOffset,
            Data d)
    {
        return PathFinder<(T native, T foreign)>
            .FindPath(start, dest,
                v => GetFaceNeighbors(v, getNeighbors, isForeign, isNative),
                (t, r) => 1f,
                (t, r) => getOffset(t.native, r.native).Length()
            );
    }
    public static IEnumerable<(T native, T foreign)> 
        GetFaceNeighbors<T>((T native, T foreign) face,
            Func<T, IEnumerable<T>> getNeighbors,
            Func<T, bool> isForeign, 
            Func<T, bool> isNative)
    {
        return getNeighbors(face.native)
            .Intersect(getNeighbors(face.foreign))
            .Where(n => isNative(n) || isForeign(n))
            .Select(n =>
            {
                if (isForeign(n))
                {
                    return (face.native, n);
                }

                return (n, face.foreign);
            });
    }
    
}