
using System;
using System.Collections.Generic;
using System.Linq;

public class ArmyDefendEdge : ICombatGraphEdge
{
    public Army Army { get; private set; }
    public CellCombatNode CellCombatNode { get; private set; }
    public List<(Unit unit, float proportion)> Defenders { get; private set; }
    public static ArmyDefendEdge ConstructAndAddToGraph(
        Army army,
        Cell defending,
        CombatCalculator combat, Data d)
    {
        var cellNode = CellCombatNode.GetOrConstruct(combat.Graph, defending, d);
        var edges = combat.Graph.GetEdgesBetween(army, cellNode);
        if (edges is not null
            && edges.FirstOrDefault(e => e is ArmyDefendEdge)
                is ArmyDefendEdge def)
        {
            return def;
        }
        var e = new ArmyDefendEdge(army, cellNode);
        combat.Graph.AddEdge(cellNode, army, e, d);
        return e;
    }
    protected ArmyDefendEdge(Army army, CellCombatNode cellCombatNode)
    {
        Army = army;
        CellCombatNode = cellCombatNode;
        Defenders = new List<(Unit unit, float proportion)>();
    }
}