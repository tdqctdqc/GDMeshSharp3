
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class UnitMover
{
    public static void MoveToPoint(this Unit unit,
        Vector2 target,
        ref Vector2 pos, ref float movePoints, Data d)
    {
        var alliance = unit.Regime.Entity(d).GetAlliance(d);
        var unitWp = d.Context.UnitWaypoints[unit];
        var found = d.Military.WaypointGrid
            .TryGetClosest(target, out var destWp,
                wp => PathFinder.IsLandPassable(wp, alliance, d));
        if (found == false)
        {
            GD.Print("coudlnt find passable closest dest wp, teleporting");
            pos = target;
            return;
        }
        if (destWp == unitWp 
            || destWp.Neighbors.Contains(unitWp.Id))
        {
            MoveDirectlyTowardsPoint(ref pos, ref movePoints,
                target, d);
        }
        else
        {
            var path = PathFinder
                .FindLandWaypointPath(unitWp, 
                    destWp, alliance, d);
            if (path == null)
            {
                GD.Print("couldn't find waypoint path, teleporting");
                pos = target;
                return;
            }
            else
            {
                MoveFromPointToPath(unit, ref pos, ref movePoints, path, d);
            }
        }
    }

    private static void MoveAlongPath(this Unit unit,
        ref Vector2 pos,
        ref float movePoints,
        Waypoint currWp,
        List<Waypoint> path,
        Data d)
    {
        if (movePoints > 0)
        {
            var index = path.IndexOf(currWp);
            while (movePoints > 0f && index < path.Count - 1)
            {
                currWp = path[index];
                var next = path[index + 1];
                var reached = MoveFromWaypointToWaypoint(
                    unit, ref pos, 
                    ref movePoints,
                    currWp, next, d);
                if (reached) index++;
            }
        }
    }
    public static void MoveFromPointToPath(this Unit unit, 
        ref Vector2 pos, 
        ref float movePoints, 
        List<Waypoint> path,
        Data d)
    {
        var currWp = d.Context.UnitWaypoints[unit];
        if (path.Contains(currWp) == false)
        {
            var p = pos;
            var closestWp = path
                .MinBy(wp => wp.Pos.GetOffsetTo(p, d).Length());
            if (currWp.Neighbors.Contains(closestWp.Id))
            {
                Vector2 closestP;
                if (closestWp == path.Last())
                {
                    closestP = closestWp.Pos;
                }
                else
                {
                    var index = path.IndexOf(closestWp);
                    var next = path[index + 1];
                    var toNext = closestWp.Pos.GetOffsetTo(next.Pos, d);
                    closestP = closestWp.Pos 
                               + closestWp.Pos.GetOffsetTo(pos, d)
                                   .GetClosestPointOnLineSegment(Vector2.Zero, toNext);
                }
                var reached = MoveDirectlyTowardsPoint(ref pos, ref movePoints, closestP, d);
                if (reached) currWp = closestWp;
                else return;
            }
            else
            {
                var pathToPath = PathFinder.FindLandWaypointPath(
                    currWp, closestWp,
                    unit.Regime.Entity(d).GetAlliance(d),
                    d);
                if (pathToPath == null)
                {
                    GD.Print($"{unit.Regime.Entity(d).Name} couldn't find path from {currWp.Id} to {closestWp.Id}");
                    throw new Exception();
                }
                var reached = MoveFromPointToWaypoint(unit, ref pos, ref movePoints,
                    pathToPath[0], d);
                if(reached) currWp = closestWp;
            }
        }
        if (movePoints > 0)
        {
            MoveAlongPath(unit, ref pos, ref movePoints, 
                currWp, path, d);
        }
    }
    private static bool MoveFromPointToWaypoint(Unit unit, 
        ref Vector2 pos, ref float movePoints,
        Waypoint target, Data d)
    {
        if (movePoints <= 0f) return pos == target.Pos;
        var alliance = unit.Regime.Entity(d).GetAlliance(d);
        var toTarget = pos.GetOffsetTo(target.Pos, d);
        var toUnit = -toTarget;
        
        var roadMult = 1f;
        foreach (var n in target.TacNeighbors(d))
        {
            if (PathFinder.IsLandPassable(n, alliance, d) == false)
                continue;
            var toNeighbor = target.Pos.GetOffsetTo(n.Pos, d);
            if (Vector2Ext.PointIsInLineSegment(toUnit, Vector2.Zero, toNeighbor))
            {
                var road = n.GetRoadWith(target, d);
                if (road != null) roadMult = road.SpeedMult;
                break;
            }
        }
        
        var cost = toTarget.Length() / roadMult;
        if (cost > movePoints)
        {
            var ratio = movePoints / cost;
            pos = (pos + toTarget * ratio).ClampPosition(d);
            movePoints = 0;
            return false;
        }
        else
        {
            movePoints -= cost;
            pos = target.Pos;
            return true;
        }
    }
    private static bool MoveFromWaypointToWaypoint(Unit unit, 
        ref Vector2 pos, ref float movePoints,
        Waypoint from, 
        Waypoint target, Data d)
    {
        var toTarget = pos.GetOffsetTo(target.Pos, d);
        
        var roadMult = 1f;
        var road = from.GetRoadWith(target, d);
        if (road != null) roadMult = road.SpeedMult;
        
        var cost = toTarget.Length() / roadMult;
        if (cost > movePoints)
        {
            var ratio = movePoints / cost;
            pos = (pos + toTarget * ratio).ClampPosition(d);
            movePoints = 0;
            return false;
        }
        else
        {
            movePoints -= cost;
            pos = target.Pos;
            return true;
        }
    }
    private static bool MoveDirectlyTowardsPoint(ref Vector2 pos, ref float movePoints,
        Vector2 target, Data d)
    {
        var offset = pos.GetOffsetTo(target, d);
        var cost = offset.Length();
        if (cost > movePoints)
        {
            var ratio = movePoints / cost;
            pos = (pos + offset * ratio).ClampPosition(d);
            movePoints = 0;
            return false;
        }
        else
        {
            movePoints -= cost;
            pos = target;
            return true;
        }
    }
}