using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using Godot;

public static class Mover
{
    public static RefSet<CellRef> MoveArmy(Army a, Data d)
    {

        return a.LineMission.LineCells;
        
        var lineCells = a.LineMission.LineCells;
        var moveRadius = a.GetArmyMoveRadius(d);
        var overlap = moveRadius.Where(c => lineCells.Contains(c.MakeRef()));

        if (overlap.Any())
        {
            return new RefSet<CellRef>(overlap.Select(c => c.MakeRef()).ToHashSet());
        }
        else
        {
            var alliance = a.Regime.Get(d).GetAlliance(d);
            var moveType = a.MoveType(d);
            var center = d.Planet.GetAveragePosition(lineCells.Refs.Select(r => r.Get(d).GetCenter()));
            
            var closestPath = PathFinder<Cell>.FindPathMultipleEnds(
                a.GetHomeCell(d),
                c => lineCells.Contains(c.MakeRef()),
                c => c.GetNeighbors(d).Where(n => n.FriendlyControlled(alliance, d)),
                (from, to) => moveType.EdgeCost(from, to, d),
                c => center.Offset(c.GetCenter(), d).LengthSquared()
            );


            if (closestPath is null)
            {
                var issue = new CantFindPathIssue(alliance,
                    "Can't find army move path",
                    a.GetHomeCell(d), lineCells.Refs.Select(r => r.Get(d)).ToList(),
                    moveType);
                d.ClientPlayerData.Issues.Add(issue);
                return a.Cells;
            }
            var closest = closestPath.LastOrDefault(c => moveRadius.Contains(c));

            return new RefSet<CellRef>(new HashSet<CellRef> { closest.MakeRef() });
        }
    }
    public static void MoveToCell(this MapPos pos,
        MoveData moveDat, Cell dest, 
        LogicKey key)
    {
        var path = key.Data.Context.FriendlyPathCache.GetOrAdd(
            (moveDat.MoveType, moveDat.Alliance, pos.GetCell(key.Data), dest));
            
        if (path == null)
        {
            var issue = new CantFindPathIssue(
                moveDat.Alliance,
                "", pos.GetCell(key.Data),
                dest.Yield().ToList(), moveDat.MoveType
            ); 
            key.Data.ClientPlayerData.Issues.Add(issue);
            return;
        }
        pos.MoveAlongPath(moveDat, path, key);
    }
    private static void MoveAlongPath(this MapPos pos, 
        MoveData moveDat, List<Cell> path, 
        LogicKey key)
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