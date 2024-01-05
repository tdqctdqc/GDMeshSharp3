using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class Mover
{
    public static void MoveToPoint(this MapPos pos, 
        MoveType moveType,
        Alliance alliance,
        Vector2 target,
        bool goThruHostile,
        ref float movePoints, LogicWriteKey key)
    {
        var startPos = target;
        inner(ref movePoints);
        CheckIfHaventMoved(pos, moveType, alliance, goThruHostile, target, startPos, 
            "moving to point", key);
        void inner(ref float movePoints)
        {
            if (pos.Pos == target)
            {
                return;
            }
            var startPos = pos.Pos;
            var d = key.Data;
            var goStraightToDist = 100f; //todo base on closest passable wps to destWp
            if (pos.Pos.GetOffsetTo(target, d).Length() <= goStraightToDist)
            {
                pos.MoveDirectlyTowardsPoint(moveType, ref movePoints, target, key);
                return;
            }
        
            pos.GetOntoCloseWaypoint(alliance, moveType, target, goThruHostile,
                ref movePoints, key);
            if (movePoints <= 0f) return;
        
            if (pos.Pos.GetOffsetTo(target, d).Length() <= goStraightToDist)
            {
                pos.MoveDirectlyTowardsPoint(moveType, ref movePoints, target, key);
                return;
            }
        
            var foundTargetWp = d.Military.WaypointGrid
                .TryGetClosest(target, out var destWp,
                    wp => moveType.Passable(wp, alliance, goThruHostile, d));
            if (foundTargetWp == false) throw new Exception();
        
            pos.FindPathAndMoveAlong(moveType,
                alliance, goThruHostile, ref movePoints, destWp, key);
            if (movePoints <= 0f) return;

            pos.MoveDirectlyTowardsPoint(moveType, ref movePoints,
                target, key);
        }
    }
    private static void CheckIfHaventMoved(MapPos pos, 
        MoveType moveType, Alliance alliance, bool goThruHostile,
        Vector2 target, Vector2 startPos, string msg,
        LogicWriteKey key)
    {
        if (pos.Pos == startPos)
        {
            var issue = new UnitNotMovingIssue(target, 
                startPos, msg, moveType, alliance, goThruHostile);
            key.Data.ClientPlayerData.Issues.Add(issue);
            pos.Set((Vector2I)target, key);
        }
    }
    public static void MoveToWaypoint(this MapPos pos, Alliance alliance,
        MoveType moveType, bool goThruHostile,
        ref float movePoints, Waypoint dest, 
        LogicWriteKey key)
    {
        var startPos = pos.Pos;
        inner(ref movePoints);
        CheckIfHaventMoved(pos, moveType, alliance, goThruHostile, dest.Pos, startPos,
            "moving to waypoint", key);
        void inner(ref float movePoints)
        {
            pos.GetOntoCloseWaypoint(alliance, moveType, 
                dest.Pos, goThruHostile, ref movePoints, key);
            pos.FindPathAndMoveAlong(moveType, alliance, goThruHostile, ref movePoints, dest, key);
        }
    }
    public static void MoveOntoAndAlongPath(this MapPos pos, 
        Alliance alliance,
        MoveType moveType, bool goThruHostile,
        ref float movePoints, 
        List<Waypoint> path, LogicWriteKey key)
    {
        var startPos = pos.Pos;
        Waypoint closeOnPath = null;
        (Waypoint, string) temp = (null, "");

        inner(ref movePoints);
        CheckIfHaventMoved(pos, moveType, alliance, goThruHostile, temp.Item1.Pos, startPos,
            temp.Item2, key);
        void inner(ref float movePoints)
        {
            if (movePoints <= 0f) return;
            var d = key.Data;
            if (pos.OnWaypointAxis())
            {
                var (w1, w2) = pos.GetAxisWps(d);
                if (path.Contains(w1))
                {
                    closeOnPath = w1;
                    temp = (closeOnPath, "going along path on wp axis");
                    pos.MoveDirectlyFromWaypointToWaypoint(moveType,
                        ref movePoints, w2, w1, key);
                }
                else if (path.Contains(w2))
                {
                    closeOnPath = w2;
                    temp = (closeOnPath, "going along path on wp axis");

                    pos.MoveDirectlyFromWaypointToWaypoint(moveType,
                        ref movePoints, w1, w2, key);
                }
            }
            else if (pos.OnWaypoint())
            {
                var wp = pos.GetWaypoint(d);
                if (path.Contains(wp))
                {
                    closeOnPath = wp;
                    temp = (closeOnPath, "going along path on wp");
                }
            }

            if (movePoints <= 0f) return;
            if (closeOnPath == null)
            {
                closeOnPath = path
                    .MinBy(p => p.Pos.GetOffsetTo(pos.Pos, d).Length());
                temp = (closeOnPath, "going towards path");


                pos.MoveToWaypoint(alliance, moveType, goThruHostile, 
                    ref movePoints, closeOnPath, key);
            }
        
            if (movePoints <= 0f) return;

            var index = path.IndexOf(closeOnPath);
            for (var i = index; i < path.Count - 1; i++)
            {
                var speed = pos.MoveDirectlyFromWaypointToWaypoint(moveType,
                    ref movePoints,
                    path[i], path[i + 1], key);    
                temp = (path[i + 1], "going along path wp to wp speed " + speed);
                if (movePoints <= 0f) return;
            }
        }
    }
    

    private static void GetOntoCloseWaypoint(this MapPos pos, 
        Alliance a,
        MoveType moveType,
        Vector2 target,
        bool goThruHostile,
        ref float movePoints, LogicWriteKey key)
    {
        if (pos.OnWaypoint()) return;
        var d = key.Data;
        if (pos.OnWaypointAxis())
        {
            var (wp1, wp2) = pos.GetAxisWps(d);
            var closeToTargetWp = wp1.Pos.GetOffsetTo(target, d) < wp1.Pos.GetOffsetTo(pos.Pos, d)
                ? wp1
                : wp2;
            var farFromTargetWp = closeToTargetWp == wp1 ? wp2 : wp1;
            pos.MoveDirectlyFromWaypointToWaypoint(moveType, ref movePoints,
                farFromTargetWp,  closeToTargetWp, key);
        }
        else 
        {
            var foundCloseWp = d.Military.WaypointGrid
                .TryGetClosest(pos.Pos, out var closeWp,
                    wp => moveType.Passable(wp, a, goThruHostile, d));
            if (foundCloseWp == false) throw new Exception();
            pos.MoveDirectlyFromPointToWaypoint(moveType, closeWp,
                ref movePoints, key);
        }
    }
    private static void FindPathAndMoveAlong(this MapPos pos,
        MoveType moveType,
        Alliance alliance, 
        bool goThruHostile,
        ref float movePoints, Waypoint dest,
        LogicWriteKey key)
    {
        if (movePoints <= 0f) return;
        if (pos.OnWaypoint() == false) throw new Exception();
        var d = key.Data;
        var wp = pos.GetWaypoint(d);
        List<Waypoint> path = PathFinder.FindPath(moveType,
            alliance,
            wp, dest, goThruHostile, d);
        if (path == null)
        {
            var issue = new CantFindWaypointPathIssue(pos.Pos,
                alliance, "",
                wp, dest, moveType, goThruHostile);
            d.ClientPlayerData.Issues.Add(issue);
            pos.Set(dest, key);
            return;
        }
        for (var i = 0; i < path.Count - 1; i++)
        {
            pos.MoveDirectlyFromWaypointToWaypoint(moveType,
                ref movePoints, path[i], path[i + 1], key);
            if (movePoints <= 0f) return;
        }
    }
    private static void MoveDirectlyFromPointToWaypoint(
        this MapPos pos,
        MoveType moveType,
        Waypoint target,
        ref float movePoints, LogicWriteKey key)
    {
        var arrived = pos.MoveDirectlyTowardsPoint(moveType,
            ref movePoints, target.Pos, key);
        if (arrived)
        {
            pos.Set(target, key);
        }
    }
    private static float MoveDirectlyFromWaypointToWaypoint(
        this MapPos pos, MoveType moveType, 
        ref float movePoints,
        Waypoint from, 
        Waypoint target, LogicWriteKey key)
    {
        var d = key.Data;
        var toTarget = pos.Pos.GetOffsetTo(target.Pos, d);
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

        if (speed == 0f)
        {
            var pt = pos.GetTri(d);
            GD.Print($"base speed {moveType.BaseSpeed} " +
                     $"\nroad speed mod {roadMult}" +
                     $"\nroad speed override {roadSpeedOverride}" +
                     $"\nlandform {pt.Landform(d).Name}" +
                     $"\nvegetation {pt.Vegetation(d).Name}" +
                    $"\nterrain speed mod {terrainSpeedMod}");
        }
        
        var cost = toTarget.Length() / speed;
        if (cost > movePoints)
        {
            var ratio = movePoints / cost;
            var newPos = (pos.Pos + toTarget * ratio).ClampPosition(d);
            pos.Set(from, target, (Vector2I)newPos, key);
            movePoints = 0;
        }
        else
        {
            movePoints -= cost;
            pos.Set(target, key);
        }

        return speed;
    }
    private static bool MoveDirectlyTowardsPoint(
        this MapPos pos, MoveType moveType,
        ref float movePoints,
        Vector2 target, LogicWriteKey key)
    {
        var d = key.Data;
        var offset = pos.Pos.GetOffsetTo(target, d);
        
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