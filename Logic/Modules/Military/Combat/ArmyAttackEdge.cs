
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ArmyAttackEdge : ICombatGraphEdge
{
    public List<(Unit unit, float proportion)> Attackers { get; private set; }
    public Army Army { get; private set; }
    public CellCombatNode CellCombatNode { get; private set; }
    public static ArmyAttackEdge ConstructAndAddToGraph(
        Army army,
        Cell target,
        CombatCalculator combat, Data d)
    {
        var cellNode = CellCombatNode.GetOrConstruct(combat.Graph, target, d);
        var edges = combat.Graph.GetEdgesBetween(army, cellNode);
        if (edges is not null 
            && edges.Any(e => e is ArmyAttackEdge))
        {
            throw new Exception();
        }
        var e = new ArmyAttackEdge(army, cellNode);
        combat.Graph.AddEdge(cellNode, army, e, d);
        var defendingArmies 
            = d.Military.UnitAux.ArmiesByOccupancy[target];
        if (defendingArmies is not null)
        {
            foreach (var def in defendingArmies)
            {
                ArmyDefendEdge.ConstructAndAddToGraph(def, target, combat, d);
            }
        }
        
        return e;
    }
    protected ArmyAttackEdge(Army army, 
        CellCombatNode cellCombatNode)
    {
        Army = army;
        CellCombatNode = cellCombatNode;
        Attackers = new List<(Unit unit, float proportion)>();
    }
}