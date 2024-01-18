using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using Godot;

public static class Mover
{
    public static void MoveToPoint(this MapPos pos, 
        MoveData moveDat,
        Vector2 target,
        LogicWriteKey key)
    {
        
    }
    public static void MoveToWaypoint(this MapPos pos,
        MoveData moveDat, PolyCell dest, 
        LogicWriteKey key)
    {
        // IMapPathfindNode start = new PointPathfindNode(pos.Pos, moveDat.MoveType, moveDat.Alliance, key.Data);
        // var path = PathFinder.FindTacticalPath(start, dest, moveDat.Alliance, moveDat.MoveType, key.Data);
        // if (path == null)
        // {
        //     var issue = new CantFindPathIssue(pos.Pos,
        //         moveDat.Alliance, "", start, dest, moveDat.MoveType);
        //     key.Data.ClientPlayerData.Issues.Add(issue);
        //     pos.Set(dest, moveDat, key);
        //     return;
        // }
        // pos.MoveAlongPathTactical(moveDat, path, key);
    }

    public static void MoveOntoAndAlongStrategicPath(this MapPos pos, 
        MoveData moveDat,
        List<PolyCell> orderPath, LogicWriteKey key)
    {
        // if (moveDat.MovePoints <= 0f) return;
        // var d = key.Data;
        // List<Waypoint> actualPath;
        // (int from, int to) lastOnAxis = (-1, -1);
        // for (var i = orderPath.Count - 1; i >= 1; i--)
        // {
        //     var from = orderPath[i - 1];
        //     var to = orderPath[i];
        //     if (onAxis(pos.Pos, from, to))
        //     {
        //         lastOnAxis = (i - 1, i);
        //         break;
        //     }
        // }
        // if (lastOnAxis.from != -1)
        // {
        //     actualPath = orderPath.GetRange(lastOnAxis.from, 
        //             orderPath.Count - lastOnAxis.from);
        // }
        // else
        // {
        //     var closeOnPath = orderPath
        //         .MinBy(wp =>
        //         {
        //             return wp.Pos.GetOffsetTo(pos.Pos, d).Length()
        //                 // + wp.Pos.GetOffsetTo(orderPath[orderPath.Count - 1].Pos, key.Data).Length()
        //                 ;
        //         });
        //     MoveToWaypoint(pos, moveDat, closeOnPath, key);
        //     if (moveDat.MovePoints == 0f) return;
        //     var i = orderPath.IndexOf(closeOnPath);
        //     actualPath = orderPath.GetRange(i, orderPath.Count - i);
        // }
        // pos.MoveAlongPathStrategic(moveDat, actualPath, key);
        //
        // bool onAxis(Vector2 p, Waypoint from, Waypoint to)
        // {
        //     return p == from.Pos 
        //            || from.Pos.GetOffsetTo(p, key.Data).Normalized()
        //                 == from.Pos.GetOffsetTo(to.Pos, key.Data).Normalized();
        // }
    }
    
    private static void MoveAlongPathTactical(this MapPos pos, 
        MoveData moveDat, List<PolyCell> path, 
        LogicWriteKey key)
    {
        // var d = key.Data;
        // for (var i = 0; i < path.Count - 1; i++)
        // {
        //     if (moveDat.MovePoints <= 0f) break;
        //     var from = path[i];
        //     var to = path[i + 1];
        //     var axis = pos.Pos.GetOffsetTo(to.Pos, d);
        //     var currTri = pos.GetTri(d);
        //     var costPerL = moveDat.MoveType
        //         .TerrainCostInstantaneous(currTri, d);
        //     var axisCost = axis.Length() * costPerL;
        //     if (axisCost > moveDat.MovePoints)
        //     {
        //         var ratio = moveDat.MovePoints / axisCost;
        //         var newPos = pos.Pos + ratio * axis;
        //         newPos = newPos.ClampPosition(d);
        //         pos.Set(newPos, moveDat, key);
        //         moveDat.MovePoints -= ratio * axisCost;
        //     }
        //     else
        //     {
        //         pos.Set(to.Pos, moveDat, key);
        //         moveDat.MovePoints -= axisCost;
        //     }
        // }
    }
    private static void MoveAlongPathStrategic(this MapPos pos, 
        MoveData moveDat, List<PolyCell> path, 
        LogicWriteKey key)
    {
        // var d = key.Data;
        // for (var i = 0; i < path.Count - 1; i++)
        // {
        //     if (moveDat.MovePoints <= 0f) break;
        //     var from = path[i];
        //     var to = path[i + 1];
        //     var axis = pos.Pos.GetOffsetTo(to.Pos, d);
        //     var currTri = pos.GetTri(d);
        //     var axisCost = moveDat.MoveType
        //         .StratMoveEdgeCost(from, to, d);
        //     if (axisCost > moveDat.MovePoints)
        //     {
        //         var ratio = moveDat.MovePoints / axisCost;
        //         var newPos = pos.Pos + ratio * axis;
        //         newPos = newPos.ClampPosition(d);
        //         pos.Set(newPos, moveDat, key);
        //         moveDat.MovePoints -= ratio * axisCost;
        //     }
        //     else
        //     {
        //         pos.Set(to.Pos, moveDat, key);
        //         moveDat.MovePoints -= axisCost;
        //     }
        // }
    }
}