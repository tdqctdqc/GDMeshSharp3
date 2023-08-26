using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Priority_Queue;

public static class PathFinder
{
    public static List<MapPolygon> FindRoadBuildPath(MapPolygon s1, MapPolygon s2, Data data,
        bool international)
    {
        return PathFinder<MapPolygon>.FindPath(s1, s2, 
            p => p.Neighbors.Items(data).Where(n => n.IsLand),
            (p, q) => BuildRoadEdgeCost(p, q, data, international), 
            (p1, p2) => p1.GetOffsetTo(p2, data).Length());
    }
    public static List<Waypoint> FindNavPath(MapPolygon s1, MapPolygon s2, Data data)
    {
        var nav = data.Planet.Nav;
        var w1 = nav.GetPolyCenterWaypoint(s1);
        var w2 = nav.GetPolyCenterWaypoint(s2);

        if (s1.IsLand && s2.IsLand)
        {
            var path = PathFinder<Waypoint>.FindPath(w1, w2, 
                p => p.Neighbors
                    .Select(nId => nav.Waypoints[nId])
                    .Where(n => n.WaypointData.Value() is SeaNav == false),
                (w,p) => EdgeRoughnessCost(w, p, data), 
                (p1, p2) => data.Planet.GetOffsetTo(p1.Pos, p2.Pos).Length());
            if (path == null)
            {
                GD.Print("bad land path " + s1.Id + " " + s2.Id);
                foreach (var wp in s1.GetAssocWaypoints(data))
                {
                    foreach (var nWp in wp.Neighbors.Select(n => nav.Waypoints[n]))
                    {
                        if (nWp.Neighbors.Contains(wp.Id) == false) 
                            throw new Exception("unsymmetrical waypoint");
                    }
                }
            }
            else return path;
        }
        return PathFinder<Waypoint>.FindPath(w1, w2, 
            p => p.Neighbors
                .Select(nId => nav.Waypoints[nId]),
            (w,p) => EdgeRoughnessCost(w, p, data), 
            (p1, p2) => data.Planet.GetOffsetTo(p1.Pos, p2.Pos).Length());
    }
    
    public static float BuildRoadEdgeCost(MapPolygon p1, MapPolygon p2, Data data, bool international = true)
    {
        if (p1.IsWater() || p2.IsWater()) return Mathf.Inf;
        if (international == false && p1.Regime.RefId != p2.Regime.RefId) return Mathf.Inf;

        var path = data.Planet.Nav.GetPolyPath(p1, p2);
        var cost = 0f;
        for (int i = 0; i < path.Count() - 1; i++)
        {
            var from = path.ElementAt(i);
            var to = path.ElementAt(i + 1);
            cost += EdgeRoughnessCost(from, to, data);
        }
        
        return cost;
    }
    
    
    public static float LandEdgeCost(Waypoint p1, Waypoint p2, Data data)
    {
        var d1 = p1.WaypointData.Value();
        var d2 = p2.WaypointData.Value();
        
        if (d1 is SeaNav) return Mathf.Inf;
        if (d2 is SeaNav) return Mathf.Inf;
        
        
        var cost = data.Planet.GetOffsetTo(p1.Pos, p2.Pos).Length();
        if (d1 is LandNav n1)
        {
            cost *= 1f + n1.Roughness;
        }
        if (d2 is LandNav n2)
        {
            cost *= 1f + n2.Roughness;
        }
        
        return cost;
    }
    public static float EdgeRoughnessCost(Waypoint p1, Waypoint p2, Data data)
    {
        var d1 = p1.WaypointData.Value();
        var d2 = p2.WaypointData.Value();
        
        
        var cost = data.Planet.GetOffsetTo(p1.Pos, p2.Pos).Length();
        var roughCost = 0f;
        if (d1 is LandNav n1)
        {
            roughCost += 1f + n1.Roughness;
        }
        if (d2 is LandNav n2)
        {
            roughCost += 1f + n2.Roughness;
        }
        return cost + roughCost * roughCost;
    }
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
