
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static partial class PathFinder
{
    
    public static List<PolyCell> FindPath(
        MoveType moveType, 
        Alliance alliance,
        PolyCell start,
        PolyCell dest, 
        Data d)
    {
        return PathFinder<PolyCell>.FindPath(start, dest, 
            p => p.GetNeighbors(d)
                .Where(wp => moveType.Passable(wp, alliance, d)),
            (w, v) => moveType.EdgeCost(w, v, d), 
            (p1, p2) => p1.GetCenter().GetOffsetTo(p2.GetCenter(), d).Length());
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
    public static float RoadBuildEdgeCost(PolyCell p1, PolyCell p2, Data data)
    {
        if (p1 is LandCell l1 == false) return Mathf.Inf;
        if (p2 is LandCell l2 == false) return Mathf.Inf;
        
        var cost = p1.GetCenter().GetOffsetTo(p2.GetCenter(), data).Length();
        cost *= 1f + l1.GetLandform(data).MinRoughness;
        cost *= 1f + l2.GetLandform(data).MinRoughness;
        
        return cost * 3f;
    }
    public static float EdgeRoughnessCost(PolyCell p1, PolyCell p2, Data data)
    {
        var cost = p1.GetCenter().GetOffsetTo(p2.GetCenter(), data).Length();
        var roughCost = 0f;
        if (p1 is LandCell n1)
        {
            roughCost += 1f + n1.GetLandform(data).MinRoughness;
        }
        if (p2 is LandCell n2)
        {
            roughCost += 1f + n2.GetLandform(data).MinRoughness;
        }
        return cost + roughCost * roughCost;
    }
}
