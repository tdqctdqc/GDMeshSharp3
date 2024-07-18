
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class ArmyExt
{
    public static ArmyTree GetTree(this Army a, int startColumn,
        Client c)
    {
        return new ArmyTree(a, 0, c);
    }

    public static HashSet<Cell> GetArmyMoveRadius(this Army a, Data d)
    {
        var moveType = a.MoveType(d);
        var alliance = a.Regime.Get(d).GetAlliance(d);
        return PathFinder<Cell>.FindFlood(
            a.GetHomeCell(d),
            c => c.GetNeighbors(d).Where(c => c.FriendlyControlled(alliance, d)),
            (from, to) => moveType.EdgeCost(from, to, d),
            moveType.BaseSpeed
        );
    }
}