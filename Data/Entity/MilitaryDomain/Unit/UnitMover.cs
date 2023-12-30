
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class UnitMover
{
    public static void MoveToPoint(this Unit unit,
        Vector2 target,
        UnitPos pos, ref float movePoints, LogicWriteKey key)
    {
        var d = key.Data;
        var goStraightToDist = 100f; //todo base on closest passable wps to destWp
        if (pos.Pos.GetOffsetTo(target, d).Length() <= goStraightToDist)
        {
            unit.MoveDirectlyTowardsPoint(pos, ref movePoints, target, key);
            return;
        }
        
        unit.GetOntoCloseWaypoint(target, pos, ref movePoints, key);
        if (movePoints <= 0f) return;
        
        if (pos.Pos.GetOffsetTo(target, d).Length() <= goStraightToDist)
        {
            unit.MoveDirectlyTowardsPoint(pos, ref movePoints, target, key);
            return;
        }
        
        var foundTargetWp = d.Military.WaypointGrid
            .TryGetClosest(target, out var destWp,
                wp => wp.IsPassableByUnit(unit));
        if (foundTargetWp == false) throw new Exception();
        
        unit.FindPathAndMoveAlong(pos, ref movePoints, destWp, key);
        if (movePoints <= 0f) return;

        unit.MoveDirectlyTowardsPoint(pos, ref movePoints,
            target, key);
    }
    
    public static void MoveToWaypoint(this Unit unit,
        UnitPos pos, ref float movePoints, Waypoint dest, 
        LogicWriteKey key)
    {
        unit.GetOntoCloseWaypoint(dest.Pos, pos,
            ref movePoints, key);
        unit.FindPathAndMoveAlong(pos, ref movePoints, dest, key);
    }
    public static void MoveOntoAndAlongPath(this Unit unit,
        UnitPos pos, ref float movePoints, 
        List<Waypoint> path, LogicWriteKey key)
    {
        var d = key.Data;
        Waypoint closeOnPath = null;
        if (pos.OnWaypointAxis())
        {
            var (w1, w2) = pos.GetAxisWps(d);
            if (path.Contains(w1))
            {
                closeOnPath = w1;
                unit.MoveDirectlyFromWaypointToWaypoint(pos,
                    ref movePoints, w2, w1, key);
            }
            else if (path.Contains(w2))
            {
                closeOnPath = w2;
                unit.MoveDirectlyFromWaypointToWaypoint(pos,
                    ref movePoints, w1, w2, key);
            }
        }
        else if (pos.OnWaypoint())
        {
            var wp = pos.GetWaypoint(d);
            if (path.Contains(wp))
            {
                closeOnPath = wp;
            }
        }

        if (movePoints <= 0f) return;
        if (closeOnPath == null)
        {
            closeOnPath = path
                .MinBy(p => p.Pos.GetOffsetTo(pos.Pos, d).Length());
            unit.MoveToWaypoint(pos, ref movePoints, closeOnPath, key);
        }
        
        if (movePoints <= 0f) return;

        var index = path.IndexOf(closeOnPath);
        for (var i = index; i < path.Count - 1; i++)
        {
            unit.MoveDirectlyFromWaypointToWaypoint(pos,
                ref movePoints,
                path[i], path[i + 1], key);    
            if (movePoints <= 0f) return;
        }
    }
    

    private static void GetOntoCloseWaypoint(this Unit unit,
        Vector2 target,
        UnitPos pos, ref float movePoints, LogicWriteKey key)
    {
        if (unit.Position.OnWaypoint()) return;
        var d = key.Data;
        if (pos.OnWaypointAxis())
        {
            var (wp1, wp2) = pos.GetAxisWps(d);
            var closeToTargetWp = wp1.Pos.GetOffsetTo(target, d) < wp1.Pos.GetOffsetTo(pos.Pos, d)
                ? wp1
                : wp2;
            var farFromTargetWp = closeToTargetWp == wp1 ? wp2 : wp1;
            unit.MoveDirectlyFromWaypointToWaypoint(pos, ref movePoints,
                farFromTargetWp,  closeToTargetWp, key);
        }
        else 
        {
            var foundCloseWp = d.Military.WaypointGrid
                .TryGetClosest(pos.Pos, out var closeWp,
                    wp => wp.IsPassableByUnit(unit));
            if (foundCloseWp == false) throw new Exception();
            unit.MoveDirectlyFromPointToWaypoint(closeWp, pos,
                ref movePoints, key);
        }
    }
    private static void FindPathAndMoveAlong(this Unit unit,
        UnitPos pos, ref float movePoints, Waypoint dest,
        LogicWriteKey key)
    {
        if (movePoints <= 0f) return;
        if (pos.OnWaypoint() == false) throw new Exception();
        var d = key.Data;
        List<Waypoint> path = PathFinder.FindPathForUnit(unit,
            pos.GetWaypoint(d), dest, d);
        if (path == null)
        {
            // GD.Print($"{unit.Regime.Entity(d).Name} " +
            //          $"couldn't find path from {pos.GetWaypoint(d).Id} " +
            //          $"to {dest.Id}, teleporting");
            pos.Set(dest, key);
            return;
        }
        for (var i = 0; i < path.Count - 1; i++)
        {
            unit.MoveDirectlyFromWaypointToWaypoint(pos,
                ref movePoints, path[i], path[i + 1], key);
            if (movePoints <= 0f) return;
        }
    }
    private static void MoveDirectlyFromPointToWaypoint(this Unit unit,
        Waypoint target,
        UnitPos pos, ref float movePoints, LogicWriteKey key)
    {
        var arrived = unit.MoveDirectlyTowardsPoint(pos,
            ref movePoints, target.Pos, key);
        if (arrived)
        {
            pos.Set(target, key);
        }
    }
    private static bool MoveDirectlyFromWaypointToWaypoint(this Unit unit, 
        UnitPos pos, ref float movePoints,
        Waypoint from, 
        Waypoint target, LogicWriteKey key)
    {
        var d = key.Data;
        var toTarget = pos.Pos.GetOffsetTo(target.Pos, d);

        var moveType = unit.Template.Entity(d)
            .MoveType.Model(d);
        var roadMult = 1f;
        var roadSpeedOverride = 0f;
        var terrainSpeedMod = 1f;
        if (moveType.UseRoads)
        {
            var road = from.GetRoadWith(target, d);
            if (road != null)
            {
                roadMult = road.SpeedMult;
                roadSpeedOverride = road.SpeedOverride;
            }
            else
            {
                terrainSpeedMod = moveType.TerrainSpeedMod(pos.GetTri(d), d);
            }
        }

        var speed = moveType.BaseSpeed
                    * terrainSpeedMod
                    * roadMult;
        speed = Mathf.Max(roadSpeedOverride, speed);
        
        var cost = toTarget.Length() / speed;
        if (cost > movePoints)
        {
            var ratio = movePoints / cost;
            var newPos = (pos.Pos + toTarget * ratio).ClampPosition(d);
            pos.Set(from, target, (Vector2I)newPos, key);
            movePoints = 0;
            return false;
        }
        else
        {
            movePoints -= cost;
            pos.Set(target, key);
            return true;
        }
    }
    private static bool MoveDirectlyTowardsPoint(
        this Unit unit,
        UnitPos pos, 
        ref float movePoints,
        Vector2 target, LogicWriteKey key)
    {
        var d = key.Data;
        var offset = pos.Pos.GetOffsetTo(target, d);
        var moveType = unit.Template.Entity(d)
            .MoveType.Model(d);
        var speed = moveType.BaseSpeed * moveType.TerrainSpeedMod(pos.GetTri(d), d);
        
        var cost = offset.Length() / speed;
        
        if (cost > movePoints)
        {
            var ratio = movePoints / cost;
            var newPos = (pos.Pos + offset * ratio).ClampPosition(d);
            pos.Set((Vector2I)newPos, key);
            movePoints = 0;
            return false;
        }
        else
        {
            movePoints -= cost;
            pos.Set((Vector2I)target, key);
            return true;
        }
    }
}