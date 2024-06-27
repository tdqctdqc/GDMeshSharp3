
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CellDefenseNode : ICombatGraphNode, IUnitNode
{
    public CellRef Cell { get; private set; }
    public List<UnitCombatInfo> UnitInfos { get; private set; }
    public int Id { get; }
    public void Add(Unit unit, Data d)
    {
        var info = new UnitCombatInfo(unit, d);
        UnitInfos.Add(info);
    }

    public bool DefendersForcedBack { get; private set; }
    public static float LossRatioToForceBack { get; private set; } 
        = .3f;
    public static int BaseFrontLength { get; private set; }
        = 1000;
    public static CellDefenseNode GetOrConstruct(CombatGraph g,
        Cell c, Data d)
    {
        if (g.CellDefNodes.TryGetValue(c.MakeRef(), out var id))
        {
            return (CellDefenseNode)g.NodesById[id];
        }
        var node = new CellDefenseNode(d.HostLogicData.CombatGraphIds.TakeId(d),
            c.MakeRef(), 
            new List<UnitCombatInfo>(),
            false
        );
        g.AddNode(node);
        g.CellDefNodes.Add(c.MakeRef(), node.Id);
        
        var armies = d.Military.UnitAux.ArmiesByOccupancy[c];
        if (armies is not null && armies.Any())
        {
            foreach (var army in armies)
            {
                g.AddEdge(army, node);
            }
        }
        
        return node;
    }

    public CellDefenseNode(int id, 
        CellRef cell, 
        List<UnitCombatInfo> unitInfos, 
        bool defendersForcedBack)
    {
        Cell = cell;
        UnitInfos = unitInfos;
        Id = id;
        DefendersForcedBack = defendersForcedBack;
    }


    public float GetPotentialDefendingPower(Data d, CombatCalculator combat)
    {
        var defenders = d.Military.UnitAux.ArmiesByOccupancy[Cell.Get(d)];
        if(defenders == null || defenders.Count == 0) return 1f;
        var val = defenders.Sum(a =>
        {
            var numEdges = combat.Graph.GetNeighbors(a)
                .Count(e => e is IUnitNode);
            return a.GetPowerPoints(d) / numEdges;
        });
        return Mathf.Max(1f, val);
    }
    public float GetPotentialAttackingPower(Data d, CombatCalculator combat)
    {
        var attackers = combat.Graph.GetNeighbors(this)
            .OfType<CellAttackNode>();

        var val = attackers.Sum(a =>
        {
            return combat.Graph.GetNeighbors(a).OfType<Army>()
                .Sum(a =>
                {
                    var numEdges = combat.Graph.GetNeighbors(a)
                        .Count(e => e is IUnitNode);
                    return a.GetPowerPoints(d) / numEdges;
                });
        });
        return Mathf.Max(1f, val);
    }

    public void CalculateCombats(CombatCalculator combat, Data d)
    {
        var attackNodes = combat.Graph.GetNeighbors(this)
            .OfType<CellAttackNode>().ToArray();
        
        if (attackNodes == null 
            || attackNodes.Any() == false
            || attackNodes.Any(n => n.UnitInfos.Any()) == false)
        {
            return;
        }
        if (UnitInfos == null 
            || UnitInfos.Any() == false)
        {
            DefendersForcedBack = true;
            return;
        }

        var attackers = attackNodes.SelectMany(n => n.UnitInfos)
            .ToArray();
        foreach (var unitCombatInfo in attackers)
        {
            doFights(unitCombatInfo, UnitInfos);
        }
        
        
        
        void doFights(UnitCombatInfo unit, List<UnitCombatInfo> targets)
        {
            var targetUnit = getTargetUnit(targets);
            if (targetUnit == null) return;
            foreach (var (troop, amt) in unit.Active.GetEnumModel(d))
            {
                var ceil = Mathf.CeilToInt(amt);
                for (var i = 0; i < ceil; i++)
                {
                    if (targetUnit.ActiveFrontSize <= 0f)
                    {
                        targetUnit = getTargetUnit(targets);
                        if (targetUnit == null) return;
                    }

                    var (targetTroop, targetTroopAmt) = getTargetTroop(targetUnit);
                    
                    if (getHit(troop, targetTroop))
                    {
                        var kill = getKillAmt(troop, targetTroop, targetTroopAmt);
                        unit.AddKill(targetTroop, kill);
                        targetUnit.AddLoss(targetTroop, kill);
                    }

                    if (getHit(targetTroop, troop))
                    {
                        var kill = getKillAmt(targetTroop, troop, 1f);
                        unit.AddLoss(troop, kill);
                        targetUnit.AddKill(troop, kill);
                    }
                }
            }
        }
        
        bool getHit(Troop troop, Troop target)
        {
            var toHit = Random.Shared.NextSingle()
                        * troop.Accuracy;
            var toEvade = Random.Shared.NextSingle()
                          * target.Evasion;
            return toHit > toEvade;
        }

        float getKillAmt(Troop troop, Troop target, float targetAmt)
        {
            var dmg = getDamage(troop, target);
            return Mathf.Min(targetAmt, dmg / target.Hitpoints);
        }
        float getDamage(Troop troop, Troop target)
        {
            var softDmg = troop.SoftAttack * (1f - target.Hardness);
            var hardDmg = troop.HardAttack * target.Hardness;
            return softDmg + hardDmg;
        }

        UnitCombatInfo getTargetUnit(List<UnitCombatInfo> targets)
        {
            var totalLength = targets.Sum(t => t.ActiveFrontSize);
            if (totalLength == 0f) return null;
            var s = Game.I.Random.RandfRange(0, totalLength - .01f);

            var i = 0;
            while (s > targets[i].ActiveFrontSize)
            {
                s -= targets[i].ActiveFrontSize;
                i++;
            }

            return targets[i];
        }

        (Troop troop, float amt) getTargetTroop(UnitCombatInfo targetUnit)
        {
            if (targetUnit.ActiveFrontSize <= 0f) return (null, 0f);
            var sample = Game.I.Random.RandfRange(0f, targetUnit.ActiveFrontSize - .01f);
            var soFar = 0f;
            foreach (var (troop, amt) in targetUnit.Active.GetEnumModel(d))
            {
                soFar += amt * troop.FrontLength;
                if (soFar >= sample) return (troop, Mathf.Min(1f, amt));
            }

            throw new Exception();
        }

        if (UnitInfos.All(info => info.ActiveFrontSize <= 0f))
        {
            DefendersForcedBack = true;
            return;
        }

        var defPower = UnitInfos.Sum(i => i.InitialPowerPoints(d));
        var lostDefPower = UnitInfos.Sum(i => i.LostPowerPoints(d));
        var defLossRatio = lostDefPower / defPower;
        if (lostDefPower / defPower >= LossRatioToForceBack)
        {
            var atkPower = attackNodes.Sum(a => a.UnitInfos.Sum(i => i.InitialPowerPoints(d)));
            var lostAtkPower = attackNodes.Sum(a => a.UnitInfos.Sum(i => i.LostPowerPoints(d)));
            var atkLossRatio = lostAtkPower / atkPower;
            if (atkLossRatio < defLossRatio)
            {
                DefendersForcedBack = true;
            }
        }
    }
    

    public void SendLosses(CombatCalculator combat, LogicWriteKey key)
    {
        foreach (var info in UnitInfos)
        {
            sendLosses(info);
        }
        foreach (var atk in combat.Graph.GetNeighbors(this)
            .OfType<CellAttackNode>())
        {
            foreach (var info in atk.UnitInfos)
            {
                sendLosses(info);
            }
        }
        void sendLosses(UnitCombatInfo info)
        {
            var unit = key.Data.Get<Unit>(info.Id);
            var proc = TroopLossesProcedure.Construct(unit);
            foreach (var (troop, amt) in info
                         .Active
                         .GetEnumModel(key.Data))
            {
                var initial = unit.Troops.Get(troop);
                proc.Losses.Add((troop.Id, initial - amt));
            }
            key.SendMessage(proc);
        }
    }
    public void DoAdvanceForVictorious(CombatCalculator combat, 
        LogicWriteKey key)
    {
        if (DefendersForcedBack)
        {
            var attackerInfos = combat.Graph.GetNeighbors(this)
                .OfType<CellAttackNode>()
                .SelectMany(n => n.UnitInfos)
                .Select(i => key.Data.Get<Unit>(i.Id));
            
            var alliancesByStr = attackerInfos
                .Where(u => key.Data.HasEntity(u.Id))
                .SortBy(u => u.Regime.Get(key.Data).GetAlliance(key.Data));
            if (alliancesByStr.Any() == false) return;

            var victoriousAllianceUnits = 
                alliancesByStr.MaxBy(kvp => kvp.Value.Sum(u => u.GetPowerPoints(key.Data)));
            
            var maxStrengthRegime = victoriousAllianceUnits.Value.SortBy(u => u.Regime.Get(key.Data))
                .MaxBy(kvp => kvp.Value.Sum(u => u.GetPowerPoints(key.Data)));
            
            var victoriousRegime = maxStrengthRegime.Key;
            var victoriousArmies = maxStrengthRegime.Value.Select(u => u.GetArmy(key.Data))
                .Distinct();
            var changeController = ConquerCellProcedure
                .Construct(Cell.Get(key.Data), victoriousRegime, victoriousArmies);
            key.SendMessage(changeController);
        }
    }

    
    
}


