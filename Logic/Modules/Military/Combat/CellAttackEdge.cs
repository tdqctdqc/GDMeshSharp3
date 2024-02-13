
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CellAttackEdge : ICombatGraphEdge
{
    ICombatGraphNode ICombatGraphEdge.Node1 => AttackNode;
    ICombatGraphNode ICombatGraphEdge.Node2 => Target;
    public CellAttackNode AttackNode { get; private set; }
    public PolyCell Target { get; private set; }
    public Dictionary<Unit, float> LossRatio { get; private set; }
    public bool DefendersForcedBack { get; private set; }
    public bool NoDefenders { get; private set; }
    public static float LossRatioToForceBack { get; private set; } = .3f;
    
    public static CellAttackEdge ConstructAndAddToGraph(
        PolyCell target,
        CombatCalculator combat, Data d)
    {
        var already = combat.Graph.GetNodeEdges(target)
            .Any(e => e is CellAttackEdge);
        if (already) throw new Exception();
        var attackNode = new CellAttackNode(target, d.HostLogicData.CombatGraphIds.TakeId(d));
        var e = new CellAttackEdge(target, attackNode);
        combat.Graph.AddEdge(e, d);
        return e;
    }

    protected CellAttackEdge(PolyCell target, CellAttackNode attackNode)
    {
        Target = target;
        AttackNode = attackNode;
        LossRatio = new Dictionary<Unit, float>();
        DefendersForcedBack = false;
        NoDefenders = false;
    }

    public void CalculateCombat(CombatCalculator combat, Data d)
    {
        var attackers = GetAttackers(combat);
        var defenders = Target.GetUnits(d);
        if (defenders == null || defenders.Count == 0)
        {
            NoDefenders = true;
            DefendersForcedBack = true;
            return;
        }
        
        foreach (var attacker in attackers)
        {
            var atkPower = attacker.Unit.GetAttackPoints(d);
            var defender = defenders.GetRandomElement();
            var defPower = defender.GetAttackPoints(d);
            var attackerHp = attacker.Unit.GetHitPoints(d);
            var attackerLossRatio = defPower / attackerHp;
            attackerLossRatio = Mathf.Clamp(attackerLossRatio, 0f, 1f);
            LossRatio.Add(attacker.Unit, attackerLossRatio);
            
            var defenderHp = defender.GetHitPoints(d);
            var defenderLossRatio = atkPower / defenderHp;
            defenderLossRatio = Mathf.Clamp(defenderLossRatio, 0f, 1f);
            LossRatio.AddOrSum(defender, defenderLossRatio);
            LossRatio[defender] = Mathf.Clamp(LossRatio[defender], 0f, 1f);
        }
        
        DefendersForcedBack = defenders.All(d => LossRatio.ContainsKey(d) && LossRatio[d] >= LossRatioToForceBack);
    }

    public void DirectResults(CombatCalculator combat, LogicWriteKey key)
    {
        if (NoDefenders)
        {
            DefendersForcedBack = true;
            return;
        }
        foreach (var (unit, ratio) in LossRatio)
        {
            if (ratio == 0f) continue;
            var proc = TroopLossesProcedure.Construct(unit);
            foreach (var (troop, amt) in unit.Troops.GetEnumerableModel(key.Data))
            {
                proc.Losses.Add((troop.Id, ratio * amt));
            }
            key.SendMessage(proc);
        }
    }

    public void InvoluntaryResults(CombatCalculator combat, LogicWriteKey key)
    {
        var defenders = Target.GetUnits(key.Data);
        if (defenders == null || defenders.Count == 0) return;
        var defenderAlliance = Target.Controller.Entity(key.Data)
            .GetAlliance(key.Data);
        var retreatCells = Target.GetNeighbors(key.Data)
            .Where(n => n.Controller.RefId == Target.Controller.RefId)
            .Where(n =>
            {
                if (combat.Graph.HasNode(n) == false) return true;
                if (combat.Graph.GetNodeEdges(n)
                        .FirstOrDefault(e => e is CellAttackEdge)
                    is CellAttackEdge e)
                {
                    return e.DefendersForcedBack == false;
                }
                return true;
            });
        foreach (var defender in defenders)
        {
            if (key.Data.Get<Unit>(defender.Id) != defender
                || LossRatio.ContainsKey(defender) == false
                || LossRatio[defender] < LossRatioToForceBack) continue;
            
            var retreatCell = retreatCells
                .FirstOrDefault(c =>
                {
                    var moveType = defender.Template.Entity(key.Data)
                        .MoveType.Model(key.Data);
                    return moveType.Passable(c, defenderAlliance, key.Data);
                });
            if (retreatCell != null)
            {
                var proc = MoveUnitProcedure.Construct(defender,
                    new MapPos(retreatCell.Id, (-1, 0f)));
                key.SendMessage(proc);
            }
            else
            {
                var proc = EntityDeletionUpdate.Create(defender.Id, key);
                key.SendMessage(proc);
            }
        }
    }

    public void VoluntaryResults(CombatCalculator combat, LogicWriteKey key)
    {
        if (DefendersForcedBack == false) return;
        
        var nonSuppressedAttackers = GetAttackers(combat)
        .Where(e => key.Data.HasEntity(e.Unit.Id))
        .Select(e => e.Unit)
        .Where(u => combat.Suppressed.Contains(u) == false);
        if (nonSuppressedAttackers.Count() == 0) return;
        var victoriousAllianceUnits = nonSuppressedAttackers
            .SortInto(u => u.Regime.Entity(key.Data).GetAlliance(key.Data))
            .MaxBy(kvp => kvp.Value.Sum(u => u.GetPowerPoints(key.Data)));
        
        var victoriousRegime = victoriousAllianceUnits.Value.SortInto(u => u.Regime.Entity(key.Data))
            .MaxBy(kvp => kvp.Value.Sum(u => u.GetPowerPoints(key.Data))).Key;
        // GD.Print($"Advance by {victoriousRegime.Name} at cell {Target.Id}");
        var changeController = ChangePolyCellControllerProcedure
            .Construct(Target, victoriousRegime);
        key.SendMessage(changeController);
        foreach (var unit in victoriousAllianceUnits.Value)
        {
            var newPosProc = MoveUnitProcedure.Construct(unit, new MapPos(Target.Id, (-1, 0f)));
            key.SendMessage(newPosProc);
        }
    }
    
    
    public bool Suppressed(CombatCalculator combat, Data d)
    {
        return false;
    }

    private IEnumerable<UnitAttackEdge> GetAttackers(CombatCalculator combat)
    {
        return combat.Graph.GetNodeEdges(AttackNode)
            .OfType<UnitAttackEdge>();
    }
}