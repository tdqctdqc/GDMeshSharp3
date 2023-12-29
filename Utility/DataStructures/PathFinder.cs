using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Priority_Queue;

public static partial class PathFinder
{

}

public static class PathFinder<T>
{
    public static List<T> FindPath(T start, T end, 
        Func<T, IEnumerable<T>> getNeighbors, 
        Func<T,T,float> getEdgeCost, 
        Func<T,T,float> heuristicFunc,
        int maxIter = Int32.MaxValue)
    {
        var open = new Priority_Queue.SimplePriorityQueue<T, float>();
        var costsFromStart = new Dictionary<T, float>();
        var closed = new HashSet<T>();
        var heuristicCosts = new Dictionary<T, float>();
        var parents = new Dictionary<T, T>();
        
        open.Enqueue(start, 0f);
        costsFromStart.Add(start, 0f);
        heuristicCosts.Add(start, heuristicFunc(start, end));
        int iter = 0;
        T current = default;
        IReadOnlyCollection<T> neighbors = null;
        bool currentHasParent = false;
        T currentParent = default;
        while(open.Count > 0 && iter < maxIter)
        {
            iter++;
            current = open.Dequeue();
            var currentCostFromStart = costsFromStart[current];

            if(current.Equals(end))
            {
                return BuildPathBackwards(current, parents);
            }

            closed.Add(current);

            //todo fix, profile, or something
            neighbors = getNeighbors(current).ToList();
            currentHasParent = parents.ContainsKey(current);
            currentParent = currentHasParent ? parents[current] : default;
            
            foreach (var n in neighbors)
            {
                if(closed.Contains(n)) continue;
                if (currentHasParent 
                    && currentParent.Equals(n)) continue; 
                var edgeCost = getEdgeCost(current, n);
                if (float.IsInfinity(edgeCost)) continue;
                if(costsFromStart.ContainsKey(n) == false)
                {
                    var costFromStart = edgeCost + 
                                        currentCostFromStart;
                    var heuristic = heuristicFunc(n, end);
                    parents.Add(n, current);
                    heuristicCosts.Add(n, heuristic);
                    open.Enqueue(n, costFromStart + heuristic);
                    costsFromStart.Add(n, costFromStart);
                }
                else
                {
                    var newCost = currentCostFromStart + edgeCost;
                    var oldCost = costsFromStart[n];
                    if(newCost < oldCost)
                    {
                        parents[n] = current;
                        open.UpdatePriority(n, newCost);
                        costsFromStart[n] = newCost;
                    }
                }
            }
        }

        return null; 
    }
    public static List<T> FindPathPredicate(T start,
        Func<T, IEnumerable<T>> getNeighbors, 
        Func<T,float> getEdgeCost, 
        Func<T,float> heuristicFunc,
        Func<T, bool> finish,
        int maxIter = Int32.MaxValue)
    {
        var open = new Priority_Queue.SimplePriorityQueue<T, float>();
        var costsFromStart = new Dictionary<T, float>();
        var closed = new HashSet<T>();
        var heuristicCosts = new Dictionary<T, float>();
        var parents = new Dictionary<T, T>();
        
        open.Enqueue(start, 0f);
        costsFromStart.Add(start, 0f);
        heuristicCosts.Add(start, heuristicFunc(start));
        int iter = 0;
        T current = default;
        IReadOnlyCollection<T> neighbors = null;
        bool currentHasParent = false;
        T currentParent = default;
        while(open.Count > 0 && iter < maxIter)
        {
            iter++;
            current = open.Dequeue();
            var currentCostFromStart = costsFromStart[current];

            if(finish(current))
            {
                return BuildPathBackwards(current, parents);
            }

            closed.Add(current);

            //todo fix, profile, or something
            neighbors = getNeighbors(current).ToList();
            currentHasParent = parents.ContainsKey(current);
            currentParent = currentHasParent ? parents[current] : default;
            
            foreach (var n in neighbors)
            {
                if(closed.Contains(n)) continue;
                if (currentHasParent 
                    && currentParent.Equals(n)) continue; 
                var edgeCost = getEdgeCost(n);
                if (float.IsInfinity(edgeCost)) continue;
                if(costsFromStart.ContainsKey(n) == false)
                {
                    var costFromStart = edgeCost + 
                                        currentCostFromStart;
                    var heuristic = heuristicFunc(n);
                    parents.Add(n, current);
                    heuristicCosts.Add(n, heuristic);
                    open.Enqueue(n, costFromStart + heuristic);
                    costsFromStart.Add(n, costFromStart);
                }
                else
                {
                    var newCost = currentCostFromStart + edgeCost;
                    var oldCost = costsFromStart[n];
                    if(newCost < oldCost)
                    {
                        parents[n] = current;
                        open.UpdatePriority(n, newCost);
                        costsFromStart[n] = newCost;
                    }
                }
            }
        }

        return null; 
    }
    public static List<T> FindPathMultipleEnds(T start, Func<T, bool> isEnd, 
        Func<T, IEnumerable<T>> getNeighbors, 
        Func<T,T,float> getEdgeCost)
    {
        int maxIters = 100_000;
        var distsFromStart = new Dictionary<T, float>();
        var parents = new Dictionary<T, T>();
        distsFromStart.Add(start, 0f);
        var open = new Priority_Queue.SimplePriorityQueue<T, float>();
        open.Enqueue(start, 0f);
        
        int iter = 0;
        while (open.Count > 0 && iter < maxIters)
        {
            var current = open.Dequeue();
            if (isEnd(current))
            {
                return BuildPathBackwards(current, parents);
            }
            iter++;

            var neighbors = getNeighbors(current);
            foreach (var n in neighbors)
            {
                var edgeCost = getEdgeCost(current, n);
                var newDistFromStart = edgeCost + distsFromStart[current];

                if (distsFromStart.ContainsKey(n) == false)
                {
                    open.Enqueue(n, newDistFromStart);
                    distsFromStart.Add(n, newDistFromStart);
                    parents.Add(n, current);
                }
                else if (newDistFromStart < distsFromStart[n])
                {
                    distsFromStart[n] = newDistFromStart;
                    parents[n] = current;
                }
            }
        }
        return null;
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

    public static float GetPathCost<T>(List<T> path, Func<T, T, float> cost)
    {
        var res = 0f;
        for (var i = 0; i < path.Count - 1; i++)
        {
            res += cost(path[i], path[i + 1]);
        }

        return res;
    }
    private static List<T> BuildPathBackwards(PathFinderNode<T> endNode)
    {
        var path = new List<T>();
        var current = endNode;
        while(current.Parent != null)
        {
            path.Add(current.Element);
            current = current.Parent;
        }
        path.Add(current.Element);
        path.Reverse();
        return path;
    }
}
