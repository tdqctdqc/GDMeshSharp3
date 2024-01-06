
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static partial class PathFinder
{
    public static List<Waypoint> FindPath(MoveType moveType, 
        Alliance alliance,
        Waypoint start,
        Waypoint dest, 
        bool goThruHostile,
        Data d)
    {
        return PathFinder<Waypoint>.FindPath(start, dest, 
            p => p.TacNeighbors(d)
                .Where(wp => moveType.Passable(wp, alliance, goThruHostile, d)),
            (w, v) => moveType.PathfindCost(w, v, alliance, goThruHostile, d), 
            (p1, p2) => p1.Pos.GetOffsetTo(p2.Pos, d).Length() / moveType.BaseSpeed);
    }
    
    public static List<TNode> FindPathFromGraph<TNode, TEdge>(TNode s1,
        TNode s2, 
        Graph<TNode, TEdge> graph, 
        Func<TEdge, float> getCost,
        Func<TNode, Vector2> getPos,
        Data data)
    {
        return PathFinder<TNode>.FindPath(s1, s2, 
            p => graph.GetNeighbors(p),
            (p, q) => getCost(graph.GetEdge(p, q)), 
            (p1, p2) => getPos(p1).GetOffsetTo(getPos(p2), data).Length());
    }
    public static List<MapPolygon> FindPathFromPolyGraph(MapPolygon s1,
        MapPolygon s2, 
        Graph<MapPolygon, float> costs, 
        Data data)
    {
        return PathFinder<MapPolygon>.FindPath(s1, s2, 
            p => costs.GetNeighbors(p),
            (p, q) => costs.GetEdge(p, q), 
            (p1, p2) => p1.GetOffsetTo(p2, data).Length());
    }

    public static float RoadBuildPolyEdgeCost(MapPolygon p1, MapPolygon p2, Data d)
    {
        var riverMult = 1f;
        if (p1.GetEdge(p2, d).IsRiver()) riverMult = 2f;
        return RoadBuildPolyCost(p1, d) + RoadBuildPolyCost(p2, d);
    }
    public static float RoadBuildPolyCost(MapPolygon p, Data d)
    {
        if (p.IsWater()) return Mathf.Inf;
        return p.Roughness;
    }
    public static float RoadBuildEdgeCost(Waypoint p1, Waypoint p2, Data data)
    {
        if (p1 is ILandWaypoint l1 == false) return Mathf.Inf;
        if (p2 is ILandWaypoint l2 == false) return Mathf.Inf;
        
        var cost = p1.Pos.GetOffsetTo(p2.Pos, data).Length();
        cost *= 1f + l1.Roughness;
        cost *= 1f + l2.Roughness;
        
        return cost * 3f;
    }
    public static float EdgeRoughnessCost(Waypoint p1, Waypoint p2, Data data)
    {
        var cost = p1.Pos.GetOffsetTo(p2.Pos, data).Length();
        var roughCost = 0f;
        if (p1 is ILandWaypoint n1)
        {
            roughCost += 1f + n1.Roughness;
        }
        if (p2 is ILandWaypoint n2)
        {
            roughCost += 1f + n2.Roughness;
        }
        return cost + roughCost * roughCost;
    }
}
