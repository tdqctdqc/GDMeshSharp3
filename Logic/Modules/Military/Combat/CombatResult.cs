
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class CombatResult
{
    public EntityRef<Unit> Unit { get; private set; }
    public Dictionary<int, float> LossesByTroopId { get; private set; }
    public Vector2 ResultOffset { get; set; }

    public static CombatResult Construct(Unit u, CombatCalculator.CombatCalcData cData,
        Data d)
    {
        if (cData.AttackNodes.ContainsKey(u) == false
            && cData.DefendNodes.ContainsKey(u) == false)
        {
            return null;
        }
        var r = new CombatResult(u.MakeRef(), new Dictionary<int, float>(),
            Vector2.Zero);
        r.RegisterLosses(cData, d);
        return r;
    }
    [SerializationConstructor] private CombatResult(EntityRef<Unit> unit, Dictionary<int, float> lossesByTroopId, Vector2 resultOffset)
    {
        Unit = unit;
        LossesByTroopId = lossesByTroopId;
        ResultOffset = resultOffset;
    }
    
    public bool HeldPosition(CombatCalculator.CombatCalcData cData,
        Data d)
    {
        var u = Unit.Entity(d);
        if (cData.DefendNodes.ContainsKey(u) == false)
        {
            return true;
        }
        var defendNode = (LandDefendNode)cData.DefendNodes[u];
        return defendNode.Held;
    }

    public bool SuccessfulAttack(CombatCalculator.CombatCalcData cData,
        Data d)
    {
        var u = Unit.Entity(d);
        if (cData.AttackNodes.ContainsKey(u) == false) return false;
        var attackNode = cData.AttackNodes[u];
        var defendNodes = cData.Graph
            .GetNeighbors(attackNode);  
        if (defendNodes.Count == 0)
        {
            return true;
        }
        else if (defendNodes.All(n => ((LandDefendNode)n).Held == false))
        {
            return true;
        }
        return false;
    }

    public void RegisterLosses(CombatCalculator.CombatCalcData cData,
        Data d)
    {
        var u = Unit.Entity(d);

        if (cData.AttackNodes.ContainsKey(u))
        {
            var attackNode = cData.AttackNodes[u];
            var ns = cData.Graph
                .GetNeighbors(attackNode);
            var enemyPower = 0f;
            foreach (var n in ns)
            {
                if (n is DefendNode == false) throw new Exception(); 
                var attackEdge = cData.Graph.GetEdge(attackNode, n);
                foreach (var atkLoss in attackEdge.AttackerLosses)
                {
                    LossesByTroopId.AddOrSum(atkLoss.Key.Id, atkLoss.Value);
                }
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