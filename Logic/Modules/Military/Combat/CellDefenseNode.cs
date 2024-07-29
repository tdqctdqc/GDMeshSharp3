
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

    public IEnumerable<CellAttackNode> GetAttackNodes(CombatGraph graph)
    {
        return graph.GetNeighbors(this).OfType<CellAttackNode>();
    }
    public IEnumerable<UnitCombatInfo> GetAttackers(CombatGraph graph)
    {
        return GetAttackNodes(graph)
            .SelectMany(n => n.UnitInfos);
    }

    public float GetPotentialDefendingPower(Data d, 
        CombatCalculator combat)
    {
        var defenders = d.Military.UnitAux.ArmiesByOccupancy[Cell.Get(d)];
        if(defenders == null || defenders.Count == 0) return 1f;
        var val = defenders.Sum(a =>
        {
            var numEdges = combat.Graph.GetNeighbors(a)
                .Count(e => e is IUnitNode);
            return a.GetPowerPointsWeighted(d) / numEdges;
        });
        return Mathf.Max(1f, val);
    }
    public float GetPotentialAttackingPower(Data d, CombatCalculator combat)
    {
        var attackers = GetAttackNodes(combat.Graph);

        var val = attackers.Sum(a =>
        {
            return combat.Graph.GetNeighbors(a).OfType<Army>()
                .Sum(a =>
                {
                    var numEdges = combat.Graph.GetNeighbors(a)
                        .Count(e => e is IUnitNode);
                    return a.GetPowerPointsWeighted(d) / numEdges;
                });
        });
        return Mathf.Max(1f, val);
    }
    
    public void CalculateCombats(CombatCalculator combat, 
        Data d)
    {
        var attackNodes = GetAttackNodes(combat.Graph);
        
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

        var cell = Cell.Get(d);
        var lf = cell.Landform.Get(d);
        var veg = cell.Vegetation.Get(d);

        var attackers = GetAttackers(combat.Graph)
            .ToArray();

        DefendersForcedBack = MilUtil.CalculateCombat(
            attackers, UnitInfos.ToArray(),
            lf, veg, d);
    }
    

    public void SendLosses(CombatCalculator combat, LogicKey key)
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
            var unit = info.Unit.Get(key.Data);
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
        LogicKey key)
    {
        if (DefendersForcedBack)
        {
            var attackerInfos = combat.Graph.GetNeighbors(this)
                .OfType<CellAttackNode>()
                .SelectMany(n => n.UnitInfos)
                .Select(i => i.Unit.Get(key.Data));
            
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


