
using System;
using System.Linq;
using Godot;

public class LandAttackAction : AttackAction
{
    public static float AdvanceDist { get; private set; } = 20f;
    public Vector2 AttackDest { get; private set; }

    public LandAttackAction(Vector2 attackDest)
    {
        AttackDest = attackDest;
    }

    public override Unit[] GetCombatGraphTargets(Unit u, Data d)
    {
        var axis = u.Position.Pos.GetOffsetTo(AttackDest, d);
        var alliance = u.Regime.Entity(d).GetAlliance(d);
        var hostiles = d.Military.UnitAux.UnitGrid
            .GetWithin(u.Position.Pos, u.Radius() * 1.5f)
            .Where(h =>
            {
                var offset = u.Position.Pos.GetOffsetTo(h.Position.Pos, d);
                var angle = axis.AngleTo(offset);
                if (angle < Mathf.Pi / 2f) return false;
                var lerp = Mathf.Lerp(0f, 1f, angle / (Mathf.Pi / 2f));
                var dist = lerp * u.Radius() * 1.5f;
                return offset.Length() <= dist;
            })
            .Where(h => h.Regime.Entity(d).GetAlliance(d)
                .IsHostileTo(alliance));
        return hostiles.ToArray();
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
        var result = CombatResult.Construct(u, cData, d);

        if (cData.DefendNodes.ContainsKey(u))
        {
            if (cData.DefendNodes[u].Held == false)
            {
                var retreatAxis = -u.Position.Pos
                    .GetOffsetTo(AttackDest, d).Normalized();
                result.ResultOffset = retreatAxis * DefendAction.RetreatDist;
                return result;
            }
        }

        var defendNodes = cData.Graph
            .GetNeighbors(attackNode);
        if (defendNodes.All(n => ((DefendNode)n).Held == false))
        {
            var advanceAxis = u.Position.Pos
                .GetOffsetTo(AttackDest, d).Normalized();
            result.ResultOffset = advanceAxis * AdvanceDist;
            return result;
        }
        result.ResultOffset = Vector2.Zero;
        return result;
    }
}