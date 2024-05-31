
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

    public float GetPotentialDefendingPower(Data d, CombatCalculator combat)
    {
        var defenders = d.Military.UnitAux.ArmiesByOccupancy[Cell];
        if(defenders == null || defenders.Count == 0) return 1f;
        return defenders.Sum(a =>
        {
            var numEdges = combat.Graph.GetNodeEdges(a)
                .Count(e => e is ArmyAttackEdge || e is ArmyDefendEdge);
            return a.GetPowerPoints(d) / numEdges;
        });
    }
    public float GetPotentialAttackingPower(Data d, CombatCalculator combat)
    {
        var attackers = combat.Graph.GetNodeEdges(this)
            .OfType<ArmyAttackEdge>()
            .Select(e => e.Army);

        return attackers.Sum(a =>
        {
            var numEdges = combat.Graph.GetNodeEdges(a)
                .Count(e => e is ArmyAttackEdge || e is ArmyDefendEdge);
            return a.GetPowerPoints(d) / numEdges;
        });
    }
    public void DistributeResources(CombatCalculator combat, Data d)
    {
        
    }

    public void CalculateCombat(CombatCalculator combat, Data d)
    {
        var atkEdges = combat.Graph.GetNodeEdges(this)
            .OfType<ArmyAttackEdge>();
        Attackers = atkEdges
            .SelectMany(a => a.Attackers
                .Select(v => new UnitCombatMemo(v.unit, v.proportion)))
            .ToList();
        var defEdges = combat.Graph.GetNodeEdges(this)
            .OfType<ArmyDefendEdge>();
        Defenders = defEdges
            .SelectMany(a => a.Defenders
                .Select(v => new UnitCombatMemo(v.unit, v.proportion)))
            .ToList();
        if (Attackers == null || Attackers.Any() == false)
        {
            GD.Print("no attackers at " + Cell.Id);
            return;
        }
        if (Defenders == null || Defenders.Any() == false)
        {
            GD.Print("no defenders at " + Cell.Id);
            DefendersForcedBack = true;
            return;
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

            var max = victoriousAllianceUnits.Value.SortBy(u => u.Unit.Regime.Get(key.Data))
                .MaxBy(kvp => kvp.Value.Sum(u => u.Unit.GetPowerPoints(key.Data)));
            
            var victoriousRegime = max.Key;
            var victoriousArmies = max.Value.Select(u => u.Unit.GetGroup(key.Data))
                .Distinct();
            var changeController = ConquerCellProcedure
                .Construct(Cell, victoriousRegime, victoriousArmies);
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