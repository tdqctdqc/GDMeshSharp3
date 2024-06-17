using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CombatCalculator
{
    public CombatGraph Graph { get; private set; }
    public void Calculate(LogicWriteKey key)
    {
        var history = CombatHistory.Construct(key.Data.BaseDomain.GameClock.Tick);
        Graph = new CombatGraph(this);
        key.Data.HostLogicData.CombatGraphIds.Reset();
        SetupGraph(key);
        
        doFor<Army>(
            army => army.DistributeResources(this, key.Data));
        doFor<CellCombatNode>(
            node => node.CalculateCombat(this, key.Data));
        
        history.DoCombatStage(Graph);
        
        doFor<CellCombatNode>(
            node => node.SendLosses(key));
        var defeatedArmies = CalculateArmyRetreats(key);
        doFor<Army>(
            army => army.RemoveIfOverrunOrDestroyed(this, key));
        doFor<CellCombatNode>(
            node => node.DoAdvanceForVictorious(this, key));
        HandleSplitArmies(defeatedArmies, key);

        var historyProc = new AddCombatHistoryProc(history);
        key.SendMessage(historyProc);

        void doFor<TType>(Action<TType> act)
        {
            foreach (var t in Graph.GetNodes().OfType<TType>())
            {
                act(t);
            }
        }
    }

    

    private HashSet<ERef<Army>> CalculateArmyRetreats(LogicWriteKey key)
    {
        var lostCells = Graph.CellCombatNodes.Values
            .OfType<CellCombatNode>()
            .Where(n => n.DefendersForcedBack)
            .Select(n => n.Cell)
            .ToHashSet();
        var defeatedArmies = new HashSet<ERef<Army>>();
        foreach (var cell in lostCells)
        {
            var alliance = cell.Controller.Get(key.Data)
                .GetAlliance(key.Data);
            var node = Graph.CellCombatNodes[cell];
            
            var armies = key.Data.Military.UnitAux
                .ArmiesByOccupancy[cell]
                ?.Select(a => a.MakeRef()).ToArray();
            if (armies is null) continue;
            defeatedArmies.AddRange(armies);
            var validNs = cell.GetNeighbors(key.Data)
                .Where(c => c.FriendlyControlled(alliance, key.Data))
                .Where(c => lostCells.Contains(c) == false)
                .Select(c => c.MakeRef())
                .ToArray();
            GD.Print("number retreat cells " + validNs.Length);
            var proc = new ArmiesRetreatProcedure(
                cell.MakeRef(), validNs, armies);
            key.SendMessage(proc);
        }

        return defeatedArmies;
    }

    private void HandleSplitArmies(HashSet<ERef<Army>> defeated,
        LogicWriteKey key)
    {
        if (defeated.Count == 0) return;
        var cells = key.Data.Planet.MapAux
            .CellHolder.Cells.Values
            .OfType<LandCell>();
        var unionsByAlliance = UnionFind.Find(cells,
            (c, d) => c.FriendlyControlled(d.Controller.Get(key.Data), key.Data),
            c => c.GetNeighbors(key.Data).OfType<LandCell>())
            .Select(u => u.ToHashSet())
            .SortBy(c => c.First().Controller.Get(key.Data).GetAlliance(key.Data));
        foreach (var eRef in defeated)
        {
            if (key.Data.HasEntity(eRef.RefId) == false) continue;
            var army = eRef.Get(key.Data);
            var armyCells = army.GetCells(key.Data);
            var flood = FloodFill<Cell>.GetFloodFill(
                armyCells.First(), armyCells.Contains, c => c.GetNeighbors(key.Data));
            if (flood.Count == army.Cells.Count()) continue;
            GD.Print("splitting army");
            var regime = army.Regime.Get(key.Data);
            var alliance = regime.GetAlliance(key.Data);
            var armyCellUnions = UnionFind.Find(armyCells,
                (c, d) => true,
                c => c.GetNeighbors(key.Data))
                .ToDictionary(v => v, v => new List<Unit>());
            Assigner.AssignRanked(
                armyCellUnions,
                l => 1f,
                v => v.Value,
                u => u.GetPowerPoints(key.Data),
                army.Units.Entities(key.Data).ToHashSet(),
                (v, u) => v.Value.Add(u),
                (v, u) => 1f
            );
            bool first = true;
            foreach (var (unionCells, units) in armyCellUnions)
            {
                if (first)
                {
                    first = false;
                    
                    continue;
                }

                var newArmy = Army.Create(regime,
                    unionCells,
                    new int[] { },
                    key
                );
                foreach (var unit in units)
                {
                    key.SendMessage(new SetUnitArmyProcedure(
                        unit.MakeRef(), newArmy.MakeRef()));
                }
            }
        }
    }
    private void SetupGraph(LogicWriteKey key)
    {
        foreach (var group in key.Data.GetAll<Army>())
        {
            group.LineMission.RegisterCombatActions(group, this, key);
            foreach (var other in group.OtherOrders)
            {
                other.RegisterCombatActions(group, this, key);
            }
        }
    }
}