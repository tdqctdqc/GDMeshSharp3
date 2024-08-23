
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class ArmyExt
{
    public static void DissolveInto(this Army army, Army into, LogicKey key)
    {
        foreach (var unit in army.Units.Entities(key.Data).ToArray())
        {
            var proc = new SetUnitArmyProcedure(unit.MakeRef(),
                into.MakeRef());
            key.SendMessage(proc);
        }
        key.Remove(army);
    }
    public static float GetMoveCost(this Army a, Cell dest, Data d)
    {
        var path = a.FindArmyPath(dest, true, d);
        if (path is null) return int.MaxValue;
        return PathFinder<Cell>.GetPathCost(
            path, (c1, c2) => a.MoveType(d)
                .EdgeCost(c1, c2, d));
    }
    public static ArmyTree GetTree(this Army a, int startColumn,
        Client c)
    {
        return new ArmyTree(a, 0, c);
    }

    public static HashSet<Cell> GetArmyMoveRadius(this Army a, Data d)
    {
        var moveType = a.MoveType(d);
        var regime = a.Regime.Get(d);
        return PathFinder<Cell>.FindFlood(
            a.GetHomeCell(d),
            c => c.GetNeighbors(d).Where(c => c.FriendlyControlled(regime, d)),
            (from, to) => moveType.EdgeCost(from, to, d),
            moveType.BaseSpeed
        );
    }
}