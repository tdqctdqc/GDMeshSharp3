using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class Mover
{
    public class MoveData
    {
        public int Id;
        public MoveType MoveType;
        public float MovePoints;
        public bool GoThruHostile;
        public Alliance Alliance;

        public MoveData(int id, MoveType moveType, float movePoints, bool goThruHostile, Alliance alliance)
        {
            Id = id;
            MoveType = moveType;
            MovePoints = movePoints;
            GoThruHostile = goThruHostile;
            Alliance = alliance;
        }
    }
    public static void MoveToPoint(this MapPos pos, 
        MoveData moveDat,
        Vector2 target,
        LogicWriteKey key)
    {
        var startPos = target;
        inner();
        CheckIfHaventMoved(pos, moveDat, target, startPos, 
            "moving to point", key);
        void inner()
        {
            if (pos.Pos == target)
            {
                return;
            }
            var startPos = pos.Pos;
            var d = key.Data;
            var goStraightToDist = 50f; 
            if (pos.Pos.GetOffsetTo(target, d).Length() <= goStraightToDist)
            {
                pos.MoveDirectlyTowardsPoint(moveDat, target, key);
                return;
            }
        
            pos.GetOntoCloseWaypoint(moveDat, target, key);
            if (moveDat.MovePoints <= 0f) return;
        
            if (pos.Pos.GetOffsetTo(target, d).Length() <= goStraightToDist)
            {
                pos.MoveDirectlyTowardsPoint(moveDat, target, key);
                return;
            }
        
            var foundTargetWp = d.Military.WaypointGrid
                .TryGetClosest(target, out var destWp,
                    wp => moveDat.MoveType.Passable(wp, moveDat.Alliance, moveDat.GoThruHostile, d));
            if (foundTargetWp == false) throw new Exception();
        
            pos.FindPathAndMoveAlong(moveDat, destWp, key);
            if (moveDat.MovePoints <= 0f) return;

            pos.MoveDirectlyTowardsPoint(moveDat, target, key);
            return;
        }
    }
    private static void CheckIfHaventMoved(MapPos pos, 
        MoveData moveDat,
        Vector2 target, Vector2 startPos, string msg,
        LogicWriteKey key)
    {
        if (pos.Pos == startPos)
        {
            var issue = new UnitNotMovingIssue(target, 
                startPos, msg, moveDat.MoveType, moveDat.Alliance, moveDat.GoThruHostile);
            key.Data.ClientPlayerData.Issues.Add(issue);
            pos.Set((Vector2I)target, moveDat, key);
        }
    }
    public static void MoveToWaypoint(this MapPos pos,
        MoveData moveDat, Waypoint dest, 
        LogicWriteKey key)
    {
        var startPos = pos.Pos;
        inner();
        CheckIfHaventMoved(pos, moveDat, dest.Pos, startPos,
            "moving to waypoint", key);
        void inner()
        {
            pos.GetOntoCloseWaypoint(moveDat, dest.Pos, key);
            if (moveDat.MovePoints == 0) return;
            pos.FindPathAndMoveAlong(moveDat, dest, key);
        }
    }
    public static void MoveOntoAndAlongPath(this MapPos pos, 
        MoveData moveDat,
        List<Waypoint> path, LogicWriteKey key)
    {
        var startPos = pos.Pos;
        Waypoint closeOnPath = null;
        (Waypoint, string) temp = (null, "");

        inner();
        CheckIfHaventMoved(pos, moveDat, temp.Item1.Pos, startPos,
            temp.Item2, key);
        void inner()
        {
            if (moveDat.MovePoints <= 0f) return;
            var d = key.Data;

            if (pos.OnWaypointAxis())
            {
                var (w1, w2) = pos.GetAxisWps(d);
                if (path.Contains(w1))
                {
                    closeOnPath = w1;
                    temp = (closeOnPath, "going along path on wp axis");
                    pos.MoveDirectlyFromWaypointToWaypoint(moveDat, w2, w1, key);
                }
                else if (path.Contains(w2))
                {
                    closeOnPath = w2;
                    temp = (closeOnPath, "going along path on wp axis");
                    pos.MoveDirectlyFromWaypointToWaypoint(moveDat, w1, w2, key);
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

            if (moveDat.MovePoints <= 0f) return;
            
            if (closeOnPath == null)
            {
                closeOnPath = path
                    .MinBy(p => p.Pos.GetOffsetTo(pos.Pos, d).Length());
                temp = (closeOnPath, "going towards path");
                pos.MoveToWaypoint(moveDat, closeOnPath, key);
            }
        
            if (moveDat.MovePoints <= 0f) return;

            var index = path.IndexOf(closeOnPath);
            for (var i = index; i < path.Count - 1; i++)
            {
                var speed = pos.MoveDirectlyFromWaypointToWaypoint(
                    moveDat, path[i], path[i + 1], key);    
                temp = (path[i + 1], "going along path wp to wp speed " + speed);
                if (moveDat.MovePoints <= 0f) break;
            }
        }
    }
    

    private static void GetOntoCloseWaypoint(this MapPos pos, 
        MoveData moveDat,
        Vector2 target,
        LogicWriteKey key)
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
            pos.MoveDirectlyFromWaypointToWaypoint(moveDat, farFromTargetWp,  closeToTargetWp, key);
        }
        else 
        {
            var foundCloseWp = d.Military.WaypointGrid
                .TryGetClosest(pos.Pos, out var closeWp,
                    wp => moveDat.MoveType.Passable(wp, moveDat.Alliance, moveDat.GoThruHostile, d));
            if (foundCloseWp == false) throw new Exception();
            pos.MoveDirectlyFromPointToWaypoint(moveDat, closeWp, key);
        }
    }
    private static void FindPathAndMoveAlong(this MapPos pos,
        MoveData moveDat, Waypoint dest,
        LogicWriteKey key)
    {
        if (moveDat.MovePoints <= 0f) return;
        if (pos.OnWaypoint() == false) throw new Exception();
        var d = key.Data;
        var wp = pos.GetWaypoint(d);
        List<Waypoint> path = PathFinder.FindPath(moveDat.MoveType,
            moveDat.Alliance,
            wp, dest, moveDat.GoThruHostile, d);
        if (path == null)
        {
            var issue = new CantFindWaypointPathIssue(pos.Pos,
                moveDat.Alliance, "",
                wp, dest, moveDat.MoveType, moveDat.GoThruHostile);
            d.ClientPlayerData.Issues.Add(issue);
            pos.Set(dest, moveDat, key);
            return;
        }
        for (var i = 0; i < path.Count - 1; i++)
        {
            pos.MoveDirectlyFromWaypointToWaypoint(moveDat, path[i], path[i + 1], key);
            if (moveDat.MovePoints <= 0f) return;
        }
    }
    private static void MoveDirectlyFromPointToWaypoint(
        this MapPos pos,
        MoveData moveDat,
        Waypoint target, LogicWriteKey key)
    {
        var arrived = pos.MoveDirectlyTowardsPoint(moveDat, target.Pos, key);
        if (arrived)
        {
            pos.Set(target, moveDat, key);
        }
    }
    private static float MoveDirectlyFromWaypointToWaypoint(
        this MapPos pos, MoveData moveDat,
        Waypoint from, 
        Waypoint target, LogicWriteKey key)
    {
        var d = key.Data;
        var toTarget = pos.Pos.GetOffsetTo(target.Pos, d);
        var roadMult = 1f;
        var roadSpeedOverride = 0f;
        var terrainSpeedMod = 1f;
        if (moveDat.MoveType.UseRoads)
        {
            var road = from.GetRoadWith(target, d);
            if (road != null)
            {
                roadMult = road.SpeedMult;
                roadSpeedOverride = road.SpeedOverride;
            }
            else
            {
                terrainSpeedMod = moveDat.MoveType.TerrainSpeedMod(pos.GetTri(d), d);
            }
        }

        var speed = moveDat.MoveType.BaseSpeed
                    * terrainSpeedMod
                    * roadMult;
        
        speed = Mathf.Max(roadSpeedOverride, speed);

        if (speed == 0f)
        {
            var pt = pos.GetTri(d);
            GD.Print($"base speed {moveDat.MoveType.BaseSpeed} " +
                     $"\nroad speed mod {roadMult}" +
                     $"\nroad speed override {roadSpeedOverride}" +
                     $"\nlandform {pt.Landform(d).Name}" +
                     $"\nvegetation {pt.Vegetation(d).Name}" +
                    $"\nterrain speed mod {terrainSpeedMod}");
        }
        
        var cost = toTarget.Length() / speed;
        if (cost > moveDat.MovePoints)
        {
            var ratio = moveDat.MovePoints / cost;
            var newPos = (pos.Pos + toTarget * ratio).ClampPosition(d);
            pos.Set(from, target, moveDat,
                (Vector2I)newPos, key);
            moveDat.MovePoints = 0;
        }
        else
        {
            moveDat.MovePoints -= cost;
            pos.Set(target, moveDat, key);
        }

        return speed;
    }
    private static bool MoveDirectlyTowardsPoint(
        this MapPos pos, MoveData moveDat,
        Vector2 target, LogicWriteKey key)
    {
        var d = key.Data;
        var offset = pos.Pos.GetOffsetTo(target, d);
        
        var speed = moveDat.MoveType.BaseSpeed * moveDat.MoveType.TerrainSpeedMod(pos.GetTri(d), d);
        
        var cost = offset.Length() / speed;
        
        if (cost > moveDat.MovePoints)
        {
            var ratio = moveDat.MovePoints / cost;
            var newPos = (pos.Pos + offset * ratio).ClampPosition(d);
            pos.Set((Vector2I)newPos, moveDat, key);
            moveDat.MovePoints = 0;
            return false;
        }
        else
        {
            moveDat.MovePoints -= cost;
            pos.Set((Vector2I)target, moveDat, key);
            return true;
        }
    }
}