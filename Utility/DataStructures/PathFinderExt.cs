
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static partial class PathFinder
{
    
    public static List<Cell> FindPathThroughFriendly(
        MoveType moveType, 
        Regime regime,
        Cell start,
        Cell dest, 
        Data d)
    {
        return PathFinder<Cell>.FindPath(start, dest, 
            p => p.GetNeighbors(d)
                .Where(wp => moveType.PassableFriendly(wp, regime, d)),
            (w, v) => moveType.EdgeCost(w, v, d), 
            (p1, p2) => p1.GetCenter().Offset(p2.GetCenter(), d).Length());
    }
    public static List<Cell> FindPathThroughFriendlyAndRival(
        MoveType moveType, 
        Regime regime,
        Cell start,
        Cell dest, 
        Data d)
    {
        return PathFinder<Cell>.FindPath(start, dest, 
            p => p.GetNeighbors(d)
                .Where(wp => moveType.PassableFriendlyOrRival(wp, regime, d)),
            (w, v) => moveType.EdgeCost(w, v, d), 
            (p1, p2) => p1.GetCenter().Offset(p2.GetCenter(), d).Length());
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
            (p1, p2) => getPos(p1).Offset(getPos(p2), data).Length());
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
    public static float RoadBuildEdgeCost(Cell p1, Cell p2, Data data)
    {
        if (p1 is LandCell l1 == false) return Mathf.Inf;
        if (p2 is LandCell l2 == false) return Mathf.Inf;
        
        var cost = p1.GetCenter().Offset(p2.GetCenter(), data).Length();
        cost *= 1f + l1.GetLandform(data).MinRoughness;
        cost *= 1f + l2.GetLandform(data).MinRoughness;
        
        return cost * 3f;
    }
    public static float EdgeRoughnessCost(Cell p1, Cell p2, Data data)
    {
        var cost = p1.GetCenter().Offset(p2.GetCenter(), data).Length();
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



    public static List<Vector2I> FindCellBorderPath(
        Vector2I start, Vector2I end, 
        Func<Cell, Cell, bool> allowed,
        Data d)
    {
        return PathFinder<Vector2I>
            .FindPath(start, end,
                v => GetCellEdgeNeighbors(v, allowed, d),
                (v, w) => 1f,
                (v, w) =>
                {
                    var v1 = PlanetDomainExt.GetPolyCell(v.X, d);
                    var v2 = PlanetDomainExt.GetPolyCell(v.Y, d);
                    var vPos = v1.GetCenter() + v1.GetCenter().Offset(v2.GetCenter(), d) / 2f;
                    
                    var w1 = PlanetDomainExt.GetPolyCell(w.X, d);
                    var w2 = PlanetDomainExt.GetPolyCell(w.Y, d);
                    var wPos = w1.GetCenter() + w1.GetCenter().Offset(w2.GetCenter(), d) / 2f;

                    return vPos.Offset(wPos, d).Length();
                });
    }

    private static IEnumerable<Vector2I> GetCellEdgeNeighbors(
        Vector2I v, 
        Func<Cell, Cell, bool> allowed,
        Data d)
    {
        var v1 = PlanetDomainExt.GetPolyCell(v.X, d);
        var v2 = PlanetDomainExt.GetPolyCell(v.Y, d);
        for (var i = 0; i < v1.Neighbors.Count; i++)
        {
            var n1 = v1.Neighbors[i];
            for (var j = 0; j < v2.Neighbors.Count; j++)
            {
                if (n1 == v2.Neighbors[j] == false)
                {
                    continue;
                }

                var nCell = PlanetDomainExt.GetPolyCell(n1, d);
                if (allowed(v1, nCell))
                {
                    yield return nCell.GetIdEdgeKey(v1);
                }
                if (allowed(v2, nCell))
                {
                    yield return nCell.GetIdEdgeKey(v2);
                }
                break;
            }
        }
    }


    public static List<Cell> FindArmyPath(this Army a, Cell dest, bool friendly, Data d)
    {
        var home = a.GetHomeCell(d);
        var moveType = a.MoveType(d);
        var cache = friendly ? d.Context.FriendlyPathCache : d.Context.RivalPathCache;
        return cache.FindPath(moveType, a.Regime.Get(d),
            home, dest);
    }
}
