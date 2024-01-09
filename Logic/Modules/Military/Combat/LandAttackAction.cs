
using System;
using System.Linq;
using Godot;

public class LandAttackAction : AttackAction
{
    public override Unit[] GetCombatGraphTargets(Unit u, Data d)
    {
        throw new System.NotImplementedException();
    }
    public override void CalculateLosses(AttackNode attacker, 
        DefendNode defender, CombatGraphEdge edge, Data d)
    {
        var atkPower = attacker.Unit.GetAttackPoints(d) 
                       * edge.AttackerProportion;
        var defPower = defender.Unit.GetAttackPoints(d) 
                       * edge.DefenderProportion;
        var attackerHp = attacker.Unit.GetHitPoints(d);
        var attackerCasualtyRatio = defPower / attackerHp;
        attackerCasualtyRatio = Mathf.Clamp(attackerCasualtyRatio, 0f, 1f);
        
        var defenderHp = defender.Unit.GetHitPoints(d);
        var defenderCasualtyRatio = atkPower / defenderHp;
        defenderCasualtyRatio = Mathf.Clamp(defenderCasualtyRatio, 0f, 1f);
        
        foreach (var kvp in attacker.Unit.Troops.GetEnumerableModel(d))
        {
            edge.AttackerLosses[kvp.Key] = kvp.Value * attackerCasualtyRatio;
        }
        
        foreach (var kvp in defender.Unit.Troops.GetEnumerableModel(d))
        {
            edge.DefenderLosses[kvp.Key] = kvp.Value * defenderCasualtyRatio;
        }
    }
    public override CombatResult CalcResult(Unit u, 
        CombatCalculator.CombatCalcData cData, Data d)
    {
        var attackNode = cData.AttackNodes[u];
        var result = new CombatResult();
        result.RegisterLosses(cData, d);
        var totalHp = u.GetHitPoints(d);
        var lostHp = result.LossesByTroopId
            .Sum(kvp =>
            {
                var troop = d.Models.GetModel<Troop>(kvp.Key);
                return kvp.Value * troop.Hitpoints;
            });
        
        return result;
    }
}