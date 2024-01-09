
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static partial class PathFinder
{
    
    public static List<Waypoint> FindStrategicPath(
        MoveType moveType, 
        Alliance alliance,
        Waypoint start,
        Waypoint dest, 
        Data d)
    {
        return PathFinder<Waypoint>.FindPath(start, dest, 
            p => p.GetNeighbors(d)
                .Where(wp => moveType.Passable(wp, alliance, d)),
            (w, v) => moveType.StratMoveEdgeCost(start, dest, d), 
            (p1, p2) => p1.Pos.GetOffsetTo(p2.Pos, d).Length());
    }
    public static List<IMapPathfindNode> FindTacticalPath(
            IMapPathfindNode start, IMapPathfindNode dest, 
            Alliance a,
            MoveType moveType, Data d)
    {
        var destIsPoint = dest is PointPathfindNode x;
        PointPathfindNode destP = destIsPoint
            ? (PointPathfindNode)dest : null;
        
        return PathFinder<IMapPathfindNode>.FindPath(
            start,
            dest,
            getNeighbors,
            (n, m) => moveType.TerrainCostPerLength(n.Tri.Tri(d), m.Tri.Tri(d), d),
            (n, m) => n.Pos.GetOffsetTo(m.Pos, d).Length() / moveType.BaseSpeed
        );

        IEnumerable<IMapPathfindNode> getNeighbors(IMapPathfindNode n)
        {
            if (n is Waypoint wp)
            {
                if (destIsPoint && destP.Neighbors.Contains(wp))
                {
                    return wp.GetNeighbors(d)
                        .Where(wp => moveType.Passable(wp, a, d))
                        .AsEnumerable<IMapPathfindNode>()
                        .Union(destP.Yield());
                }

                return wp.GetNeighbors(d).Where(wp => moveType.Passable(wp, a, d));
            }
            if (n is PointPathfindNode p)
            {
                return p.Neighbors;
            }
            throw new Exception();
        }
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
