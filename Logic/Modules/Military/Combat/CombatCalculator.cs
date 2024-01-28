using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CombatCalculator
{
    public CombatGraph Graph { get; private set; }
    public Dictionary<(Alliance, PolyCell), CellAttackNode> CellAttackNodes { get; private set; }
    public HashSet<Unit> Suppressed { get; private set; }
    public void Calculate(LogicWriteKey key)
    {
        Graph = new CombatGraph(this);
        Suppressed = new HashSet<Unit>();
        SetupGraph(key);
        
    }

    private void SetupGraph(LogicWriteKey key)
    {
        foreach (var order in key.Data.GetAll<UnitGroup>()
                     .Select(g => g.GroupOrder))
        {
            if (order == null) continue;
            order.RegisterCombatActions(this, key);
        }
    }
    private static void CalcLosses(Data d)
    {
        // foreach (var kvp in cData.DefendNodes)
        // {
        //     var defender = kvp.Key;
        //     var defNode = kvp.Value;
        //     var totalDefPower = defender.GetAttackPoints(d);
        //     var attacks = cData.Graph
        //         .GetNeighbors(defNode).OfType<AttackNode>();
        //     foreach (var attackNode in attacks)
        //     {
        //         var edge = cData.Graph.GetEdge(defNode, attackNode);
        //         var attacker = attackNode.Unit;
        //         var attackerAction = attackNode.Action;
        //         attackerAction.CalculateLosses(attackNode, defNode, edge, d);
        //     }
        // }
        // foreach (var kvp in cData.DefendNodes)
        // {
        //     kvp.Value.DetermineDefenseResult(cData, d);
        // }
    }
}