
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ArmyAttackEdge : ICombatGraphEdge
{
    ICombatGraphNode ICombatGraphEdge.Node1 => CellNode;
    ICombatGraphNode ICombatGraphEdge.Node2 => Army;
    public CellCombatNode CellNode { get; private set; }
    public Army Army { get; private set; }
    
    public static ArmyAttackEdge ConstructAndAddToGraph(
        Army army,
        Cell target,
        CombatCalculator combat, Data d)
    {
        var cellNode = CellCombatNode.GetOrConstruct(combat.Graph, target, d);
        var already = combat.Graph.GetEdgesBetween(army, cellNode)
            .Any(e => e is ArmyAttackEdge);
        if (already) throw new Exception();
        var e = new ArmyAttackEdge(army, cellNode);
        combat.Graph.AddEdge(e, d);
        return e;
    }
    protected ArmyAttackEdge(Army army, CellCombatNode cellNode)
    {
        Army = army;
        CellNode = cellNode;
    }
}