using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Priority_Queue;

public static class PathFinder
{
    public static List<MapPolygon> FindRoadBuildPath(MapPolygon s1, MapPolygon s2, RoadModel road, Data data,
        bool international, Func<MapPolygon, MapPolygon, float> buildRoadEdgeCost = null)
    {
        if (buildRoadEdgeCost == null)
        {
            buildRoadEdgeCost = (p, q) => BuildRoadEdgeCost(p, q, road, data, international);
        }
        return PathFinder<MapPolygon>.FindPath(s1, s2, p => p.Neighbors.Entities(),
            buildRoadEdgeCost, 
            (p1, p2) => p1.GetOffsetTo(p2, data).Length());
    }

    public static float GetBuildPathCost(List<MapPolygon> path, RoadModel road, Data data, 
        bool international, Func<MapPolygon, MapPolygon, float> buildRoadEdgeCost = null)
    {
        if (buildRoadEdgeCost == null)
        {
            buildRoadEdgeCost = (p, q) => BuildRoadEdgeCost(p, q, road, data, international);
        }
        return PathFinder<MapPolygon>.GetPathCost(path, (p, q) => BuildRoadEdgeCost(p, q, road, data));
    }
    public static List<MapPolygon> FindTravelPath(MapPolygon s1, MapPolygon s2, Data data,
        Func<MapPolygon, MapPolygon, float> travelEdgeCost = null)
    {
        if (travelEdgeCost == null) travelEdgeCost = (p, q) => TravelEdgeCost(p, q, data);
        return PathFinder<MapPolygon>.FindPath(s1, s2, p => p.Neighbors.Entities(),
            travelEdgeCost, (p1, p2) => p1.GetOffsetTo(p2, data).Length());
    }

    public static float GetTravelPathCost(List<MapPolygon> path, Data data,
        Func<MapPolygon, MapPolygon, float> travelEdgeCost = null)
    {
        if (travelEdgeCost == null) travelEdgeCost = (p, q) => TravelEdgeCost(p, q, data);
        return PathFinder<MapPolygon>.GetPathCost(path, travelEdgeCost);
    }
    private static float TravelEdgeCost(MapPolygon p1, MapPolygon p2, Data data)
    {
        var e = p1.GetEdge(p2, data);
        if (p1.IsWater() || p2.IsWater()) return Mathf.Inf;
        var dist = p1.GetOffsetTo(p2, data).Length();
        if (data.Society.RoadAux.ByEdgeId[e.Id] is RoadSegment r)
        {
            return dist / r.Road.Model().Speed;
        }
        else
        {
            return dist * (p1.Roughness + p2.Roughness);
        }
    }
    
    private static float BuildRoadEdgeCost(MapPolygon p1, MapPolygon p2, RoadModel road, Data data, bool international = true)
    {
        if (p1.IsWater() || p2.IsWater()) return Mathf.Inf;
        if (international == false && p1.Regime.RefId != p2.Regime.RefId) return Mathf.Inf;
        var dist = p1.GetOffsetTo(p2, data).Length();
        return dist * (p1.Roughness + p2.Roughness) / 2f;
    }
}
public static class PathFinder<T>
{
    public static List<T> FindPath(T start, T end, 
        Func<T, IReadOnlyCollection<T>> getNeighbors, 
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
        bool hasLast = false;
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

            neighbors = getNeighbors(current);
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
