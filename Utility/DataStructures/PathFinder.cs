using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Priority_Queue;
using LightObjectPool;

public static partial class PathFinder
{

}

public static class PathFinder<T>
{
    private static Pool<PathFindInfo<T>> _pool;

    static PathFinder()
    {
        var policy = new PoolPolicy<PathFindInfo<T>>(
            f => f.GetPooledObject().Value,
            p => p.Clear(),
            100
        );
        _pool = LightObjectPool.Pool.Create<PathFindInfo<T>>(p => p.Clear(), 100);
    }

    public static List<T> FindStraightPath(T start, T end,
        Func<T, IEnumerable<T>> getNeighbors,
        Func<T, T, float> getDist,
        int maxIter = 10000)
    {
        var res = new List<T> { start };
        var curr = start;
        var endNeighbors = getNeighbors(end).ToHashSet();
        int iter = 0;
        while (iter < maxIter)
        {
            iter++;
            var ns = getNeighbors(curr);
            if (ns.Contains(end))
            {
                res.Add(end);
                return res;
            }
            var almostThereNs = ns.Where(endNeighbors.Contains);
            if (almostThereNs.Count() > 0)
            {
                var next = almostThereNs.MinBy(a => getDist(a, end));
                res.Add(next);
                curr = next;
                continue;
            }
            var dist = getDist(curr, end);
            var close = ns.Where(n => getDist(n, end) < dist);
            if (close.Count() == 0)
            {
                res.Add(end);
                return res;
            };
            var closest = close.MinBy(c => getDist(end, c));
            res.Add(closest);
            curr = closest;
        }

        return null;
    }
    
    
    public static List<T> FindPath(T start, T end, 
        Func<T, IEnumerable<T>> getNeighbors, 
        Func<T,T,float> getEdgeCost, 
        Func<T,T,float> heuristicFunc,
        int maxIter = Int32.MaxValue)
    {
        var info = _pool.Get();
        info.Open.Enqueue(start, 0f);
        info.CostsFromStart.Add(start, 0f);
        // info.HeuristicCosts.Add(start, heuristicFunc(start, end));
        int iter = 0;
        T current = default;
        IEnumerable<T> neighbors = null;
        bool currentHasParent = false;
        T currentParent = default;
        while(info.Open.Count > 0 && iter < maxIter)
        {
            iter++;
            current = info.Open.Dequeue();
            var currentCostFromStart = info.CostsFromStart[current];

            if(current.Equals(end))
            {
                var p = BuildPathBackwards(current, info.Parents);
                _pool.Return(info);
                return p;
            }

            info.Closed.Add(current);

            neighbors = getNeighbors(current);
            currentHasParent = info.Parents.ContainsKey(current);
            currentParent = currentHasParent ? info.Parents[current] : default;
            
            foreach (var n in neighbors)
            {
                if(info.Closed.Contains(n)) continue;
                if (currentHasParent 
                    && currentParent.Equals(n)) continue; 
                var edgeCost = getEdgeCost(current, n);
                if (float.IsInfinity(edgeCost)) continue;
                if(info.CostsFromStart.ContainsKey(n) == false)
                {
                    var costFromStart = edgeCost + 
                                        currentCostFromStart;
                    var heuristic = heuristicFunc(n, end);
                    info.Parents.Add(n, current);
                    info.Open.Enqueue(n, costFromStart + heuristic);
                    info.CostsFromStart.Add(n, costFromStart);
                }
                else
                {
                    var newCost = currentCostFromStart + edgeCost + heuristicFunc(current, end);
                    var oldCost = info.CostsFromStart[n];
                    if(newCost < oldCost)
                    {
                        info.Parents[n] = current;
                        info.Open.UpdatePriority(n, newCost);
                        info.CostsFromStart[n] = newCost;
                    }
                }
            }
        }
        _pool.Return(info);
        return null; 
    }
    private class PathFindInfo<T>
    {
        public Dictionary<T, float> CostsFromStart;
        public Priority_Queue.SimplePriorityQueue<T, float> Open;
        public HashSet<T> Closed;
        public Dictionary<T, T> Parents;

        public PathFindInfo()
        {
            Open = new Priority_Queue.SimplePriorityQueue<T, float>();
            CostsFromStart = new Dictionary<T, float>();
            Closed = new HashSet<T>();
            Parents = new Dictionary<T, T>();
        }

        public void Clear()
        {
            Open.Clear();
            CostsFromStart.Clear();
            Closed.Clear();
            Parents.Clear();
        }
    }
    public static T FindClosest(T start, Func<T, bool> isEnd, 
        Func<T, IEnumerable<T>> getNeighbors, 
        Func<T,T,float> getEdgeCost)
    {
        var info = _pool.Get();
        int maxIters = 100_000;
        info.CostsFromStart.Add(start, 0f);
        info.Open.Enqueue(start, 0f);
        
        int iter = 0;
        while (info.Open.Count > 0 && iter < maxIters)
        {
            var current = info.Open.Dequeue();
            if (isEnd(current))
            {
                _pool.Return(info);
                return current;
            }
            iter++;

            var neighbors = getNeighbors(current);
            foreach (var n in neighbors)
            {
                var edgeCost = getEdgeCost(current, n);
                var newDistFromStart = edgeCost + info.CostsFromStart[current];

                if (info.CostsFromStart.ContainsKey(n) == false)
                {
                    info.Open.Enqueue(n, newDistFromStart);
                    info.CostsFromStart.Add(n, newDistFromStart);
                    info.Parents.Add(n, current);
                }
                else if (newDistFromStart < info.CostsFromStart[n])
                {
                    info.CostsFromStart[n] = newDistFromStart;
                    info.Parents[n] = current;
                }
            }
        }
        _pool.Return(info);
        return default;
    }
    
    public static Graph<T, List<T>> TransformGraph<TOldEdge>(
            Graph<T, TOldEdge> oldGraph,
            Func<T, IEnumerable<T>> getNeighbors,
            Func<T,T,float> getEdgeCost,
            Func<T, T, float> heuristicCost)
    {
        var newGraph = new Graph<T, List<T>>();
        var els = oldGraph.Elements;
        for (var i = 0; i < els.Count; i++)
        {
            var element = els[i];
            newGraph.AddNode(element);
        }
        for (var i = 0; i < els.Count; i++)
        {
            var element = els[i];
            var ns = oldGraph.GetNeighbors(element)
                .Where(n => newGraph.HasEdge(element, n) == false)
                .ToHashSet();

            var dic = 
                FindMultiplePaths(
                    element, ns,
                    getNeighbors,
                    getEdgeCost,
                    heuristicCost
                );
            foreach (var kvp in dic)
            {
                var n = kvp.Key;
                var path = kvp.Value;
                newGraph.AddEdge(element, n, path);
            }
        }

        return newGraph;
    }
    
    public static Dictionary<T, List<T>> FindMultiplePaths(T end, 
        HashSet<T> starts, 
        Func<T, IEnumerable<T>> getNeighbors, 
        Func<T,T,float> getEdgeCost,
        Func<T,T,float> getHeuristic)
    {
        var res = new Dictionary<T, List<T>>();
        if (starts.Count() == 0) return res;
        if (starts.Count() == 1)
        {
            var path = FindPath(starts.First(),
                end, getNeighbors, getEdgeCost, getHeuristic);
            res.Add(starts.First(), path);
            return res;
        }
        var info = _pool.Get();
        int maxIters = 100_000;
        var count = starts.Count();
        var foundCount = 0;
        int iter = 0;
        info.Open.Enqueue(end, 0f);
        info.CostsFromStart.Add(end, 0f);
        while (foundCount < count 
               && info.Open.Count > 0
               && iter < maxIters)
        {
            iter++;
            var current = info.Open.Dequeue();
            info.Closed.Add(current);
            if (starts.Contains(current))
            {
                foundCount++;
            }
            
            var neighbors = getNeighbors(current)
                .Where(n => info.Closed.Contains(n) == false);
            foreach (var n in neighbors)
            {
                var edgeCost = getEdgeCost(current, n);
                var newDistFromStart = edgeCost + info.CostsFromStart[current];
                if (info.CostsFromStart.ContainsKey(n) == false)
                {
                    info.CostsFromStart[n] = newDistFromStart;
                    info.Parents[n] = current;
                    info.Open.Enqueue(n, newDistFromStart);
                }
                else if (info.CostsFromStart[n] > newDistFromStart)
                {
                    info.CostsFromStart[n] = newDistFromStart;
                    info.Parents[n] = current;
                    info.Open.UpdatePriority(n, newDistFromStart);
                }
            }
        }
        
        if (foundCount != starts.Count())
        {
            // GD.Print($"missed {starts.Count() - foundCount} paths");
        }
        _pool.Return(info);
        
        foreach (var start in starts)
        {
            res.Add(start, BuildPath(start, info.Parents));
        }

        return res;
    }
    
    
    
    private static List<T> BuildPathBackwards(T end, Dictionary<T, T> parents)
    {
        var path = new List<T> {end};
        var to = end;
        while (parents.ContainsKey(to))
        {
            var from = parents[to];
            path.Add(from);
            to = from;
        }

        path.Reverse();
        return path;
    }
    private static List<T> BuildPath(T end, Dictionary<T, T> parents)
    {
        var path = new List<T> {end};
        var to = end;
        while (parents.ContainsKey(to))
        {
            var from = parents[to];
            path.Add(from);
            to = from;
        }
        
        return path;
    }
    public static float GetPathCost<T>(List<T> path, Func<T, T, float> cost)
    {
        var res = 0f;
        for (var i = 0; i < path.Count - 1; i++)
        {
            res += cost(path[i], path[i + 1]);
        }

        return res;
    }
}
