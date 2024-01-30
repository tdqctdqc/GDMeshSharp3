
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


    public static List<List<FrontFace<PolyCell>>>
        FindPolyCellFront(IEnumerable<PolyCell> cells,
            Alliance alliance,
            Data d)
    {
        return FindFront<PolyCell>(
            cells.ToHashSet(),
            isForeign,
            c => c.GetNeighbors(d),
            (p,q) => p.GetCenter().GetOffsetTo(q.GetCenter(), d),
            i => d.Planet.PolygonAux.PolyCells.Cells[i]);

        bool isForeign(PolyCell c)
        {
            if (c.Controller.IsEmpty()) return false;
            var controllerRegime = c.Controller.Entity(d);
            var controllerAlliance = controllerRegime.GetAlliance(d);
            return alliance.IsRivals(controllerAlliance, d);
        }
    }
    
    
    public static List<List<FrontFace<T>>> FindFront<T>(
        HashSet<T> natives,
        Func<T, bool> isForeign,
        Func<T, IEnumerable<T>> getNeighbors,
        Func<T, T, Vector2> getOffset,
        Func<int, T> getElement)
        where T : IIdentifiable
    {
        var res = new List<List<FrontFace<T>>>();
        var oppositions = natives.SelectMany(e =>
        {
            return getNeighbors(e)
                .Where(isForeign)
                .Select(f => new FrontFace<T>(e.Id, f.Id));
        }).ToHashSet();
        
        var oppositionsHash = oppositions.ToHashSet();

        int maxIter = 10_000;
        while (oppositionsHash.Count > 0)
        {
            var first = oppositionsHash.First();
            oppositionsHash.Remove(first);
            var list = new LinkedList<FrontFace<T>>();
            list.AddFirst(first);
            go(first, t => list.AddFirst(t));
            go(first, t => list.AddLast(t));
            res.Add(list.ToList());
        }
        return res;

        void go(FrontFace<T> edge,
            Action<FrontFace<T>> add)
        {
            var native = getElement(edge.Native);
            var foreign = getElement(edge.Foreign);
            var mutualNs = edge.GetNeighbors(
                    getNeighbors, isForeign, natives.Contains, 
                    getElement)
                .Where(oppositionsHash.Contains);
            if (mutualNs.Count() == 0) return;
            
            var e = mutualNs.First();
            add(e);
            oppositionsHash.Remove(e);
            go(e, add);
        }
    }
    
    
    public static List<FrontFace<T>> 
        PathfindOnFront<T>(FrontFace<T> start,
            FrontFace<T> dest,
            Func<T, IEnumerable<T>> getNeighbors,
            Func<T, bool> isForeign, 
            Func<T, bool> isNative,
            Func<T, T, Vector2> getOffset,
            Func<int, T> getElement,
            Data d)
                where T : IIdentifiable
    {
        Func<FrontFace<T>, IEnumerable<FrontFace<T>>> getFaceNeighbors = f =>
        {
            return f.GetNeighbors(getNeighbors, isForeign, isNative, getElement);
        };
        return PathFinder<FrontFace<T>>
            .FindPath(start, dest,
                getFaceNeighbors,
                (t, r) => 1f,
                (t, r) => getOffset(getElement(t.Native), getElement(r.Native)).Length()
            );
    }
}