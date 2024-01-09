
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CombatResult
{
    public EntityRef<Unit> Unit { get; private set; }
    public Dictionary<int, float> LossesByTroopId { get; private set; }
    public Vector2 ResultPos { get; private set; }
    public void RegisterLosses(CombatCalculator.CombatCalcData cData,
        Data d)
    {
        var u = Unit.Entity(d);

        if (cData.AttackNodes.ContainsKey(u))
        {
            var attackNode = cData.AttackNodes[u];
            var ns = cData.Graph.GetNeighbors(attackNode);
            if (ns.Count != 1) throw new Exception();
            if (ns.First() is DefendNode defendNode == false) throw new Exception();
            var attackEdge = cData.Graph.GetEdge(attackNode, defendNode);
            foreach (var atkLoss in attackEdge.AttackerLosses)
            {
                LossesByTroopId.AddOrSum(atkLoss.Key.Id, atkLoss.Value);
            }
        }
        

        if (cData.DefendNodes.ContainsKey(u))
        {
            var ourDefNode = cData.DefendNodes[u];
            var edges = cData.Graph.GetNeighbors(ourDefNode)
                .Select(n => cData.Graph.GetEdge(n, ourDefNode));
            foreach (var defEdge in edges)
            {
                foreach (var defLoss in defEdge.DefenderLosses)
                {
                    LossesByTroopId.AddOrSum(defLoss.Key.Id, defLoss.Value);
                }
            }
        }
    }
}