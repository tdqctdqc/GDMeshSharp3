using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class CombatCalculator
{
    public class CombatCalcData
    {
        public Dictionary<Unit, AttackNode> AttackNodes { get; private set; }
        public Dictionary<Unit, DefendNode> DefendNodes { get; private set; }
        public Graph<CombatGraphNode, CombatGraphEdge> Graph { get; private set; }
        public Dictionary<Unit, CombatAction> Actions { get; private set; }
        public CombatCalcData(Dictionary<Unit, CombatAction> actions)
        {
            Actions = actions;
            AttackNodes = new Dictionary<Unit, AttackNode>();
            DefendNodes = new Dictionary<Unit, DefendNode>();
            Graph = new Graph<CombatGraphNode, CombatGraphEdge>();
        }
    }
    public static CombatResultsProcedure Calculate(
        Dictionary<Unit, CombatAction> actions, 
        Data d)
    {
        var cData = new CombatCalcData(actions);
        SetupGraph(cData, d);
        CalcAttackProportions(cData, d);
        CalcDefendProportions(cData, d);
        CalcLosses(cData, d);
        return CalcResults(cData, d);
    }

    private static void SetupGraph(CombatCalcData cData, Data d)
    {
        var edges = cData.Actions.AsParallel()
            .Select(a =>
                (a.Key, a.Value.GetCombatGraphTargets(a.Key, d)))
            .ToList();

        DefendNode getDefendNode(Unit u)
        {
            if (cData.DefendNodes.ContainsKey(u) == false)
            {
                DefendNode node;
                if (u.Template.Entity(d).Domain == TroopDomain.Land)
                {
                    node = new LandDefendNode(u);
                }
                else throw new Exception();
                
                cData.DefendNodes.Add(u, node);
                cData.Graph.AddNode(node);
            }
            return cData.DefendNodes[u];
        }
        foreach (var v in edges)
        {
            var actor = v.Key;
            var action = cData.Actions[actor];
            if (action is AttackAction atkAction)
            {
                var node = new AttackNode(actor, atkAction);
                cData.Graph.AddNode(node);
                cData.AttackNodes.Add(actor, node);
                var targets = v.Item2;
                if(targets == null) continue;
                foreach (var target in targets)
                {
                    cData.Graph.AddEdge(node, getDefendNode(target), new CombatGraphEdge());
                }            
            }
            
        }
    }

    private static void CalcAttackProportions(CombatCalcData cData, Data d)
    {
        foreach (var kvp in cData.AttackNodes)
        {
            var atkNode = kvp.Value;
            var attacker = kvp.Key;
            var attackProportion = 0f;
            if (cData.DefendNodes.ContainsKey(attacker))
            {
                attackProportion = .5f;
            }
            else
            {
                attackProportion = 1f;
            }

            var targets = cData.Graph.GetNeighbors(atkNode);
            var totalPower = targets.Sum(t => t.Unit.GetPowerPoints(d));
            if (totalPower == 0f) continue;
            foreach (var defendNode in targets)
            {
                var targetProportion = attackProportion * defendNode.Unit.GetPowerPoints(d) / totalPower;
                var edge = cData.Graph.GetEdge(atkNode, defendNode);
                edge.AttackerProportion = targetProportion;
            }
        }
    }

    private static void CalcDefendProportions(CombatCalcData cData, Data d)
    {
        foreach (var kvp in cData.DefendNodes)
        {
            var defNode = kvp.Value;
            var defender = kvp.Key;
            var defendProportion = 0f;
            if (cData.AttackNodes.ContainsKey(defender))
            {
                defendProportion = .5f;
            }
            else
            {
                defendProportion = 1f;
            }

            var attackers = cData.Graph.GetNeighbors(defNode);
            var totalPower = attackers.Sum(t => t.Unit.GetPowerPoints(d));
            if (totalPower == 0f) continue;
            foreach (var atkNode in attackers)
            {
                var targetProportion = defendProportion * atkNode.Unit.GetPowerPoints(d) / totalPower;
                var edge = cData.Graph.GetEdge(defNode, atkNode);
                edge.DefenderProportion = targetProportion;
            }
        }
    }
    private static void CalcLosses(CombatCalcData cData, Data d)
    {
        foreach (var kvp in cData.DefendNodes)
        {
            var defender = kvp.Key;
            var defNode = kvp.Value;
            var totalDefPower = defender.GetAttackPoints(d);
            var attacks = cData.Graph
                .GetNeighbors(defNode).OfType<AttackNode>();
            foreach (var attackNode in attacks)
            {
                var edge = cData.Graph.GetEdge(defNode, attackNode);
                var attacker = attackNode.Unit;
                var attackerAction = attackNode.Action;
                attackerAction.CalculateLosses(attackNode, defNode, edge, d);
            }
        }
        foreach (var kvp in cData.DefendNodes)
        {
            kvp.Value.DetermineDefenseResult(cData, d);
        }
    }
    private static CombatResultsProcedure CalcResults(CombatCalcData cData, Data d)
    {
        // var results = cData.Actions.AsParallel()
        //     .Select(kvp => kvp.Value.CalcResult(kvp.Key, cData, d));
        //
        
        var groupsEngaged = d.GetAll<UnitGroup>()
            .Where(g => g.Units.Items(d)
                .Any(u => cData.AttackNodes.ContainsKey(u) || cData.DefendNodes.ContainsKey(u)));
        var results = groupsEngaged.AsParallel()
            .SelectMany(g => g.GroupOrder.GetCombatResults(g, cData, d));
        var proc = CombatResultsProcedure.Construct();
        proc.Results.AddRange(results);
        return proc;
    }
}