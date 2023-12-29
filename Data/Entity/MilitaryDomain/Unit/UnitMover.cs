
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class UnitMover
{
    public static void MoveToPoint(this Unit unit,
        Vector2 target,
        UnitPos pos, ref float movePoints, Data d)
    {
        var goStraightToDist = 50f; //todo base on closest passable wps to destWp
        if (pos.Pos.GetOffsetTo(target, d).Length() <= goStraightToDist)
        {
            unit.MoveDirectlyTowardsPoint(pos, ref movePoints, target, d);
            return;
        }
        
        unit.GetOntoCloseWaypoint(target, pos, ref movePoints, d);
        if (movePoints <= 0f) return;
        
        if (pos.Pos.GetOffsetTo(target, d).Length() <= goStraightToDist)
        {
            unit.MoveDirectlyTowardsPoint(pos, ref movePoints, target, d);
            return;
        }
        
        var foundTargetWp = d.Military.WaypointGrid
            .TryGetClosest(target, out var destWp,
                wp => wp.IsPassableByUnit(unit));
        if (foundTargetWp == false) throw new Exception();
        
        unit.FindPathAndMoveAlong(pos, ref movePoints, destWp, d);
        if (movePoints <= 0f) return;

        unit.MoveDirectlyTowardsPoint(pos, ref movePoints,
            target, d);
    }
    
    public static void MoveToWaypoint(this Unit unit,
        UnitPos pos, ref float movePoints, Waypoint dest, 
        Data d)
    {
        unit.GetOntoCloseWaypoint(dest.Pos, pos,
            ref movePoints, d);
        unit.FindPathAndMoveAlong(pos, ref movePoints, dest, d);
    }
    public static void MoveOntoAndAlongPath(this Unit unit,
        UnitPos pos, ref float movePoints, 
        List<Waypoint> path, Data d)
    {
        var closeOnPath = path.MinBy(p => p.Pos.GetOffsetTo(pos.Pos, d).Length());
        unit.MoveToWaypoint(pos, ref movePoints, closeOnPath, d);
        if (movePoints <= 0f) return;

        var index = path.IndexOf(closeOnPath);
        for (var i = index; i < path.Count - 1; i++)
        {
            unit.MoveDirectlyFromWaypointToWaypoint(pos,
                ref movePoints,
                path[i], path[i + 1], d);    
            if (movePoints <= 0f) return;
        }
    }
    

    private static void GetOntoCloseWaypoint(this Unit unit,
        Vector2 target,
        UnitPos pos, ref float movePoints, Data d)
    {
        if (unit.Position.OnWaypoint()) return;
        if (pos.OnWaypointAxis())
        {
            var (wp1, wp2) = pos.GetAxisWps(d);
            var closeToTargetWp = wp1.Pos.GetOffsetTo(target, d) < wp1.Pos.GetOffsetTo(pos.Pos, d)
                ? wp1
                : wp2;
            var farFromTargetWp = closeToTargetWp == wp1 ? wp2 : wp1;
            unit.MoveDirectlyFromWaypointToWaypoint(pos, ref movePoints,
                farFromTargetWp,  closeToTargetWp, d);
        }
        else 
        {
            var foundCloseWp = d.Military.WaypointGrid
                .TryGetClosest(pos.Pos, out var closeWp,
                    wp => wp.IsPassableByUnit(unit));
            if (foundCloseWp == false) throw new Exception();
            unit.MoveDirectlyFromPointToWaypoint(closeWp, pos, ref movePoints, d);
        }
    }
    private static void FindPathAndMoveAlong(this Unit unit,
        UnitPos pos, ref float movePoints, Waypoint dest,
        Data d)
    {
        List<Waypoint> path = PathFinder.FindPathForUnit(unit,
            pos.GetWaypoint(d), dest, d);
        for (var i = 0; i < path.Count - 1; i++)
        {
            unit.MoveDirectlyFromWaypointToWaypoint(pos,
                ref movePoints, path[i], path[i + 1], d);
            if (movePoints <= 0f) return;
        }
    }
    private static void MoveDirectlyFromPointToWaypoint(this Unit unit,
        Waypoint target,
        UnitPos pos, ref float movePoints, Data d)
    {
        var arrived = unit.MoveDirectlyTowardsPoint(pos,
            ref movePoints, target.Pos, d);
        if (arrived)
        {
            pos.Set(target);
        }
    }
    private static bool MoveDirectlyFromWaypointToWaypoint(this Unit unit, 
        UnitPos pos, ref float movePoints,
        Waypoint from, 
        Waypoint target, Data d)
    {
        var toTarget = pos.Pos.GetOffsetTo(target.Pos, d);
        
        var roadMult = 1f;
        var road = from.GetRoadWith(target, d);
        if (road != null) roadMult = road.SpeedMult;
        
        var cost = toTarget.Length() / roadMult;
        if (cost > movePoints)
        {
            var ratio = movePoints / cost;
            var newPos = (pos.Pos + toTarget * ratio).ClampPosition(d);
            pos.Set(from, target, newPos);
            movePoints = 0;
            return false;
        }
        else
        {
            movePoints -= cost;
            pos.Set(target);
            return true;
        }
    }
    private static bool MoveDirectlyTowardsPoint(
        this Unit unit,
        UnitPos pos, 
        ref float movePoints,
        Vector2 target, Data d)
    {
        var offset = pos.Pos.GetOffsetTo(target, d);
        var cost = offset.Length();
        if (cost > movePoints)
        {
            var ratio = movePoints / cost;
            var newPos = (pos.Pos + offset * ratio).ClampPosition(d);
            pos.Set(newPos);
            movePoints = 0;
            return false;
        }
        else
        {
            movePoints -= cost;
            pos.Set(target);
            return true;
        }
    }
}