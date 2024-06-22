
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
    public CellCombatHistory History { get; private set; }
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
        var val = defenders.Sum(a =>
        {
            var numEdges = combat.Graph.GetNodeEdges(a)
                .Count(e => e is ArmyAttackEdge || e is ArmyDefendEdge);
            return a.GetPowerPoints(d) / numEdges;
        });
        return Mathf.Max(1f, val);
    }
    public float GetPotentialAttackingPower(Data d, CombatCalculator combat)
    {
        var attackers = combat.Graph.GetNodeEdges(this)
            .OfType<ArmyAttackEdge>()
            .Select(e => e.Army);

        var val = attackers.Sum(a =>
        {
            var numEdges = combat.Graph.GetNodeEdges(a)
                .Count(e => e is ArmyAttackEdge || e is ArmyDefendEdge);
            return a.GetPowerPoints(d) / numEdges;
        });
        return Mathf.Max(1f, val);
    }

    public void CalculateCombats(CombatCalculator combat, Data d)
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
        History = CellCombatHistory.Construct(this);
        
        if (Attackers == null || Attackers.Any() == false)
        {
            return;
        }
        if (Defenders == null || Defenders.Any() == false)
        {
            DefendersForcedBack = true;
            return;
        }
        else
        {
            var allDefenderTroops = IdCount<Troop>.Sum(
                Defenders.Select(m => m.Unit.Troops).ToArray());
            var allAttackerTroops = IdCount<Troop>.Sum(
                Attackers.Select(m => m.Unit.Troops).ToArray());

            foreach (var attacker in Attackers)
            {
                var atkPower = attacker.Unit.GetAttackPoints(d) 
                               * attacker.Proportion;
                var defender = Defenders.GetRandomElement();
                var defPower = defender.Unit.GetAttackPoints(d) 
                               * defender.Proportion;
                var attackerHp = attacker.Unit.GetHitPoints(d) 
                                 * attacker.Proportion;
                var attackerLossRatio = defPower / attackerHp;
                attackerLossRatio = Mathf.Clamp(attackerLossRatio, 
                    0f, 1f);
                attacker.ProportionLosses = attackerLossRatio;
            
                var defenderHp = defender.Unit.GetHitPoints(d) 
                                 * defender.Proportion;
                var defenderLossRatio = atkPower / defenderHp;
                defenderLossRatio = Mathf.Clamp(defenderLossRatio, 0f, 1f);
                defenderLossRatio = Mathf.Min(1f - defender.ProportionLosses,
                    defenderLossRatio);
                defender.ProportionLosses += defenderLossRatio;
                
                History.AddLossesAndKills(attacker.Unit,
                    attackerLossRatio, 
                    defender.Unit, 
                    defenderLossRatio);
            }
            DefendersForcedBack = Defenders
                .All(memo => memo.ProportionLosses >= LossRatioToForceBack);
        }
    }

    public void SendLosses(LogicWriteKey key)
    {
        foreach (var memo in Defenders)
        {
            sendLosses(memo);
        }
        foreach (var memo in Attackers)
        {
            sendLosses(memo);
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
    public void DoAdvanceForVictorious(CombatCalculator combat, 
        LogicWriteKey key)
    {
        if (DefendersForcedBack)
        {
            var alliancesByStr = Attackers
                .Where(u => key.Data.HasEntity(u.Unit.Id))
                .SortBy(u => u.Unit.Regime.Get(key.Data).GetAlliance(key.Data));
            if (alliancesByStr.Any() == false) return;

            var victoriousAllianceUnits = 
                alliancesByStr.MaxBy(kvp => kvp.Value.Sum(u => u.Unit.GetPowerPoints(key.Data)));
            
            var maxStrengthRegime = victoriousAllianceUnits.Value.SortBy(u => u.Unit.Regime.Get(key.Data))
                .MaxBy(kvp => kvp.Value.Sum(u => u.Unit.GetPowerPoints(key.Data)));
            
            var victoriousRegime = maxStrengthRegime.Key;
            var victoriousArmies = maxStrengthRegime.Value.Select(u => u.Unit.GetArmy(key.Data))
                .Distinct();
            var changeController = ConquerCellProcedure
                .Construct(Cell, victoriousRegime, victoriousArmies);
            key.SendMessage(changeController);
        }
    }


    public class UnitCombatMemo
    {
        public Unit Unit { get; set; }
        public float Proportion { get; set; }
        
        //is out of proportion engaged not whole unit
        public float ProportionLosses { get; set; }

        public UnitCombatMemo(Unit unit, float proportion)
        {
            Unit = unit;
            Proportion = proportion;
            ProportionLosses = 0f;
        }
    }
}