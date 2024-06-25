using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CombatCalculator
{
    public CombatGraph Graph { get; private set; }
    public void Calculate(LogicWriteKey key)
    {
        Graph = new CombatGraph();
        key.Data.HostLogicData.CombatGraphIds.Reset();
        SetupGraph(key);


        var distributions = key.Data.GetAll<Army>()
            .Where(a => Graph.NodesById.ContainsKey(a.Id))
            .AsParallel()
            .Select(army => army.DistributeResources(this, key.Data))
            .ToArray();
        foreach (var distribution in distributions)
        {
            foreach (var (node, units) in distribution)
            {
                foreach (var unit in units)
                {
                    node.Add(unit, key.Data);
                }
            }
        }
        
        
        
        doFor<CellDefenseNode>(
            node => node.CalculateCombats(this, key.Data));
        doFor<CellDefenseNode>(
            node => node.SendLosses(this, key));
        var defeatedArmies =
            Graph.GetNodes().OfType<Army>()
                .Where(a => a.Retreat(this, key))
                .ToHashSet();
        doFor<Army>(
            army => army.RemoveIfOverrunOrDestroyed(this, key));
        doFor<CellDefenseNode>(
            node => node.DoAdvanceForVictorious(this, key));
        HandleSplitArmies(defeatedArmies, key);

        var historyProc = new AddCombatHistoryProc(
            key.Data.BaseDomain.GameClock.Tick,
            Graph);
        key.SendMessage(historyProc);

        void doFor<TType>(Action<TType> act)
        {
            foreach (var t in Graph.GetNodes().OfType<TType>())
            {
                act(t);
            }
        }
    }

    

    

    private void HandleSplitArmies(HashSet<Army> defeated,
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
        foreach (var army in defeated)
        {
            if (key.Data.HasEntity(army.Id) == false) continue;
            var armyCells = army.GetCells(key.Data);
            var flood = FloodFill<Cell>.GetFloodFill(
                armyCells.First(), armyCells.Contains, c => c.GetNeighbors(key.Data));
            if (flood.Count == army.Cells.Count()) continue;
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