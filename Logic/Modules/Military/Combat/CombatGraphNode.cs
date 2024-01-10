using System.Linq;

public class CombatGraphNode
{
    public Unit Unit { get; private set; }
    public CombatGraphNode(Unit unit)
    {
        Unit = unit;
    }
}
public class AttackNode : CombatGraphNode
{
    public AttackAction Action { get; private set; }

    public AttackNode(Unit unit, AttackAction action) 
        : base(unit)
    {
        Action = action;
    }
}
public class DefendNode : CombatGraphNode
{
    public bool Held { get; private set; }
    public DefendNode(Unit unit) 
        : base(unit)
    {
    }

    public void DetermineIfHeld(CombatCalculator.CombatCalcData cData,
        Data d)
    {
        Held = true;
        var totalHp = Unit.GetHitPoints(d);
        var attackers = cData.Graph
            .GetNeighbors(this);
        var lostHp = 0f;
        var inflictedHp = 0f;
        var opposingHp = 0f;
        foreach (var atkNode in attackers)
        {
            var attacker = atkNode.Unit;
            foreach (var kvp in attacker.Troops.GetEnumerableModel(d))
            {
                opposingHp += kvp.Key.Hitpoints * kvp.Value;
            }
            var e = cData.Graph.GetEdge(atkNode, this);
            foreach (var kvp in e.DefenderLosses)
            {
                lostHp += kvp.Value * kvp.Key.Hitpoints;
            }
            foreach (var kvp in e.AttackerLosses)
            {
                inflictedHp += kvp.Value * kvp.Key.Hitpoints;
            }
        }

        if (cData.AttackNodes.ContainsKey(Unit))
        {
            var attackTargets = cData.Graph
                .GetNeighbors(cData.AttackNodes[Unit]);
            foreach (var n in attackTargets)
            {
                var edge = cData.Graph.GetEdge(this, n);
                foreach (var kvp in edge.AttackerLosses)
                {
                    lostHp += kvp.Key.Hitpoints * kvp.Value;
                }
            }
        }

        if (lostHp < totalHp / 10f) return;
        if (lostHp / totalHp < inflictedHp / opposingHp) return;
        Held = false;
    }
}