
using System;
using System.Linq;

public static class CombatExt
{
    public static CellAttackNode GetOrAddCellAttackNode(
        this CombatCalculator combat,
        Cell target, Data d)
    {
        var cellAttackEdges = combat.Graph
            .GetNodeEdges(target)
            .OfType<CellAttackEdge>();
        if (cellAttackEdges.Count() > 1) throw new Exception();
        var cellAttackEdge = cellAttackEdges.FirstOrDefault();
        if (cellAttackEdge == null)
        {
            cellAttackEdge = CellAttackEdge.ConstructAndAddToGraph(target, combat, d);
        }

        return cellAttackEdge.AttackNode;
    }
}