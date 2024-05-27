
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CellCombatNode : ICombatGraphNode
{
    public Cell Cell { get; private set; }
    public List<UnitCombatMemo> Attackers { get; private set; }
    public List<UnitCombatMemo> Defenders { get; private set; }
    public int Id { get; }
    public bool DefendersForcedBack { get; private set; }
    public static float LossRatioToForceBack { get; private set; } = .3f;

    public static CellCombatNode GetOrConstruct(CombatGraph g,
        Cell c, Data d)
    {
        if (g.CellCombatNodes.TryGetValue(c, out var val))
        {
            return val;
        }
        var node = new CellCombatNode(c, d.HostLogicData.CombatGraphIds.TakeId(d));
        g.CellCombatNodes.Add(c, node);
        return node;
    }
    private CellCombatNode(Cell cell, int id)
    {
        Cell = cell;
        Id = id;
        Attackers = new List<UnitCombatMemo>();
        Defenders = new List<UnitCombatMemo>();
    }
    
    
    
    
    public void CalculateCombat(CombatCalculator combat, Data d)
    {
        if (Defenders == null || Defenders.Any() == false)
        {
            DefendersForcedBack = true;
        }
        else
        {
            foreach (var attacker in Attackers)
            {
                var atkPower = attacker.Unit.GetAttackPoints(d) * attacker.Proportion;
                var defender = Defenders.GetRandomElement();
                var defPower = defender.Unit.GetAttackPoints(d) * defender.Proportion;
                var attackerHp = attacker.Unit.GetHitPoints(d) * attacker.Proportion;
                var attackerLossRatio = defPower / attackerHp;
                attackerLossRatio = Mathf.Clamp(attackerLossRatio, 0f, 1f);
                attacker.ProportionLosses = attackerLossRatio;
            
                var defenderHp = defender.Unit.GetHitPoints(d) * defender.Proportion;
                var defenderLossRatio = atkPower / defenderHp;
                defenderLossRatio += defender.ProportionLosses;
                defenderLossRatio = Mathf.Clamp(defenderLossRatio, 0f, 1f);
                defender.ProportionLosses = defenderLossRatio;
            }
            DefendersForcedBack = Defenders
                .All(memo => memo.ProportionLosses >= LossRatioToForceBack);
        }
    }

    public void DirectResults(CombatCalculator combat, LogicWriteKey key)
    {
        foreach (var memo in Defenders)
        {
            sendLosses(memo);
        }
        foreach (var memo in Attackers)
        {
            sendLosses(memo);
        }
        if (DefendersForcedBack)
        {
            var victoriousAllianceUnits = Attackers
                .SortBy(u => u.Unit.Regime.Get(key.Data).GetAlliance(key.Data))
                .MaxBy(kvp => kvp.Value.Sum(u => u.Unit.GetPowerPoints(key.Data)));

            var victoriousRegime = victoriousAllianceUnits.Value.SortBy(u => u.Unit.Regime.Get(key.Data))
                .MaxBy(kvp => kvp.Value.Sum(u => u.Unit.GetPowerPoints(key.Data))).Key;
            // GD.Print($"Advance by {victoriousRegime.Name} at cell {Target.Id}");
            var changeController = ChangePolyCellControllerProcedure
                .Construct(Cell, victoriousRegime);
            key.SendMessage(changeController);
        }
        void sendLosses(UnitCombatMemo memo)
        {
            var ratio = memo.ProportionLosses;
            if (ratio == 0f) return;
            var unit = memo.Unit;
            var proportion = memo.Proportion;
            var proc = TroopLossesProcedure.Construct(unit);
            foreach (var (troop, amt) in unit.Troops.GetEnumerableModel(key.Data))
            {
                proc.Losses.Add((troop.Id, ratio * proportion * amt));
            }
            key.SendMessage(proc);
        }
    }

    public void InvoluntaryResults(CombatCalculator combat, LogicWriteKey key)
    {
        
    }

    public void VoluntaryResults(CombatCalculator combat, LogicWriteKey key)
    {
    }

    public class UnitCombatMemo
    {
        public Unit Unit { get; set; }
        public float Proportion { get; set; }
        public float ProportionLosses { get; set; }

        public UnitCombatMemo(Unit unit, float proportion)
        {
            Unit = unit;
            Proportion = proportion;
            ProportionLosses = 0f;
        }
    }
}