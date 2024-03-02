using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using Godot;

public static class Mover
{
    public static void MoveToCell(this MapPos pos,
        MoveData moveDat, Cell dest, 
        LogicWriteKey key)
    {
        var path = key.Data.Context.PathCache.GetOrAdd(
            (moveDat.MoveType, moveDat.Alliance, pos.GetCell(key.Data), dest));
            
        if (path == null)
        {
            var issue = new CantFindPathIssue(
                moveDat.Alliance,
                "", pos.GetCell(key.Data),
                dest, moveDat.MoveType
            ); 
            key.Data.ClientPlayerData.Issues.Add(issue);
            return;
        }
        pos.MoveAlongPath(moveDat, path, key);
    }
    private static void MoveAlongPath(this MapPos pos, 
        MoveData moveDat, List<Cell> path, 
        LogicWriteKey key)
    {
        var d = key.Data;
        var index = path
            .FindIndex(c => c.Id == pos.PolyCell);
        if (index == -1) throw new Exception();
        int finalCell = pos.PolyCell;
        int finalDestCell = pos.Destination.DestCellId;
        float finalProgress = pos.Destination.Proportion;
        for (var i = index; i < path.Count - 1; i++)
        {
            if (moveDat.MovePoints <= 0f) break;
            var from = path[i];
            var to = path[i + 1];
            var progress = 0f;
            if (pos.Destination.DestCellId == to.Id)
            {
                progress = pos.Destination.Proportion;
            }
            var axisCost = moveDat.MoveType
                .EdgeCost(from, to, d) * (1f - progress);
            
            if (axisCost > moveDat.MovePoints)
            {
                finalProgress = progress + 
                    (1f - progress) * moveDat.MovePoints / axisCost;
                finalCell = from.Id;
                finalDestCell = to.Id;
                moveDat.MovePoints = 0f;
                break;
            }
            else
            {
                finalCell = to.Id;
                finalDestCell = -1;
                finalProgress = 0f;
                moveDat.MovePoints -= axisCost;
            }
        }
        pos.Set(finalCell, (finalDestCell, finalProgress), moveDat, key);
    }
}