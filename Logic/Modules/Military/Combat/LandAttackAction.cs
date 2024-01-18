
using System;
using System.Linq;
using Godot;

public class LandAttackAction : AttackAction
{
    public Vector2 AttackDest { get; private set; }
    public LandAttackAction(Vector2 attackDest)
    {
        AttackDest = attackDest;
    }
    public override Unit[] GetCombatGraphTargets(Unit u, Data d)
    {
        return new Unit[0];
        // var axis = u.Position.Pos.GetOffsetTo(AttackDest, d);
        // var alliance = u.Regime.Entity(d).GetAlliance(d);
        // var attackRadius = 20f;
        // var hostiles = d.Military.UnitAux.UnitGrid
        //     .GetWithin(u.Position.Pos, attackRadius, v => true)
        //     .Where(h =>
        //     {
        //         var offset = u.Position.Pos.GetOffsetTo(h.Position.Pos, d);
        //         // var angle = axis.AngleTo(offset);
        //         // if (angle < Mathf.Pi / 2f) return false;
        //         // var lerp = Mathf.Lerp(0f, 1f, angle / (Mathf.Pi / 2f));
        //         var dist = attackRadius;//lerp * attackRadius;
        //         var inRange = offset.Length() <= dist;
        //         return inRange;
        //     })
        //     .Where(h => h.Regime.Entity(d).GetAlliance(d)
        //         .IsHostileTo(alliance));
        // return hostiles.ToArray();
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
    
}