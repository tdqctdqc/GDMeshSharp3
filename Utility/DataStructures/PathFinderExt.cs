
using System.Collections.Generic;
using System.Linq;
using Godot;

public static partial class PathFinder
{
    public static List<Waypoint> FindPathForUnit(Unit u, 
        Waypoint start,
        Waypoint dest, Data d)
    {
        var alliance = u.Regime.Entity(d).GetAlliance(d);
        var moveType = u.Template.Entity(d).MoveType.Model(d);
        return PathFinder<Waypoint>.FindPath(start, dest, 
            p => p.TacNeighbors(d)
                .Where(wp => moveType.Passable(wp, alliance, d)),
            (w,p) => LandWpMoveCost(w, p, d), 
            (p1, p2) => p1.Pos.GetOffsetTo(p2.Pos, d).Length());
    }
    public static List<Waypoint> FindPath(UnitMoveType moveType, 
        Alliance alliance,
        Waypoint start,
        Waypoint dest, Data d)
    {
        return PathFinder<Waypoint>.FindPath(start, dest, 
            p => p.TacNeighbors(d)
                .Where(wp => moveType.Passable(wp, alliance, d)),
            (w,p) => LandWpMoveCost(w, p, d), 
            (p1, p2) => p1.Pos.GetOffsetTo(p2.Pos, d).Length());
    }
    public static bool IsPassableByUnit(this Waypoint wp, Unit u)
    {
        return true;
    }
    public static List<MapPolygon> FindRoadBuildPolyPath(MapPolygon s1, MapPolygon s2, Data data,
        bool international)
    {
        return PathFinder<MapPolygon>.FindPath(s1, s2, 
            p => p.Neighbors.Items(data).Where(n => n.IsLand),
            (p, q) => BuildRoadEdgeCost(p, q, data, international), 
            (p1, p2) => p1.GetOffsetTo(p2, data).Length());
    }
    public static List<Waypoint> FindNavPathBetweenPolygons(
        MapPolygon s1, MapPolygon s2, Data data)
    {
        var w1 = s1.GetCenterWaypoint(data);
        var w2 = s2.GetCenterWaypoint(data);

        if (s1.IsLand && s2.IsLand)
        {
            var path = PathFinder<Waypoint>.FindPath(w1, w2, 
                p => p.TacNeighbors(data)
                    .Where(n => n is SeaWaypoint == false
                    && (n is IRiverWaypoint r ? r.Bridgeable : true)),
                (w,p) => EdgeRoughnessCost(w, p, data), 
                (p1, p2) => p1.Pos.GetOffsetTo(p2.Pos, data).Length());
            return path;
        }
        return PathFinder<Waypoint>.FindPath(w1, w2, 
            p => p.TacNeighbors(data),
            (w,p) => EdgeRoughnessCost(w, p, data), 
            (p1, p2) => p1.Pos.GetOffsetTo(p2.Pos, data).Length());
    }

    public static List<Waypoint> FindLandWaypointPath(Waypoint start, Waypoint dest, 
        Alliance a, Data data)
    {
        return PathFinder<Waypoint>.FindPath(start, dest, 
            p => p.TacNeighbors(data)
                .Where(wp => IsLandPassable(wp, a, data)),
            (w,p) => LandWpMoveCost(w, p, data), 
            (p1, p2) => p1.Pos.GetOffsetTo(p2.Pos, data).Length());
    }

    private static float LandWpMoveCost(Waypoint w, Waypoint v, Data d)
    {
        
        var cost = w.Pos.GetOffsetTo(v.Pos, d).Length();
        var road = w.GetRoadWith(v, d);
        if (road != null)
        {
            cost /= road.SpeedMult;
        }
        else if(w is ILandWaypoint lw && v is ILandWaypoint lv)
        {
            var rough = (lw.Roughness + lv.Roughness) / 2f;
            cost *= (1f + rough);
        }
        
        return cost;
    }
    
    public static bool IsLandPassable(Waypoint wp, Alliance a, Data d)
    {
        // if (wp is IRiverWaypoint r)
        // {
        //     return r.Bridgeable
        //         && wp.TacNeighbors(d)
        //             .Any(n => n is ILandWaypoint && IsLandPassable(n, a, d));
        // }
        if (wp is ILandWaypoint == false) return false;
        if (wp.GetOccupyingRegime(d).GetAlliance(d) == a) return true;
        if (wp.IsControlled(a, d)) return true;
        return false;
    }
    public static float BuildRoadEdgeCost(MapPolygon p1, MapPolygon p2, Data data, bool international = true)
    {
        if (p1.IsWater() || p2.IsWater()) return Mathf.Inf;
        if (international == false && p1.OwnerRegime.RefId != p2.OwnerRegime.RefId) return Mathf.Inf;

        var path = data.Military.TacticalWaypoints.GetPolyPath(p1, p2);
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
        if (p1 is SeaWaypoint) return Mathf.Inf;
        if (p2 is SeaWaypoint) return Mathf.Inf;
        
        var cost = p1.Pos.GetOffsetTo(p2.Pos, data).Length();
        if (p1 is ILandWaypoint n1)
        {
            cost *= 1f + n1.Roughness;
        }
        if (p2 is ILandWaypoint n2)
        {
            cost *= 1f + n2.Roughness;
        }
        
        return cost;
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
