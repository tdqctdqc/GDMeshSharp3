using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class CombatCalculator
{
    public CombatGraph Graph { get; private set; }
    public void Calculate(LogicWriteKey key)
    {
        Graph = new CombatGraph();
        key.Data.HostLogicData.CombatGraphIds.Reset();
        var logger = key.Data.Logger;
        SetupGraph(key);
        DistributeResources(key, logger);
        PruneEmptyNodes(logger);
        CalculateCombats(key, logger);
        HandleCombatResults(key, logger);

        var historyProc = new AddCombatHistoryProc(
            key.Data.BaseDomain.GameClock.Tick,
            Graph);
        key.SendMessage(historyProc);
    }

    private void CalculateCombats(LogicWriteKey key, Logger logger)
    {
        logger.RunAndLogTime("Combat calcs", LogType.Logic,
            () =>
            {
                Parallel.ForEach(Graph.GetNodes().OfType<CellDefenseNode>(),
                    n => n.CalculateCombats(this, key.Data));
            });
    }

    private void HandleCombatResults(LogicWriteKey key, Logger logger)
    {
        logger.RunAndLogTime("sending losses", LogType.Logic,
            () =>
            {
                doFor<CellDefenseNode>(
                    node => node.SendLosses(this, key));
            });

        logger.RunAndLogTime("handling retreats overruns and advances",
            LogType.Logic, () =>
            {
                var defeatedArmies =
                    Graph.GetNodes().OfType<Army>()
                        .Where(a => a.Retreat(this, key))
                        .ToHashSet();
                doFor<Army>(
                    army => army.RemoveIfOverrunOrDestroyed(this, key));
                doFor<CellDefenseNode>(
                    node => node.DoAdvanceForVictorious(this, key));
                HandleSplitArmies(defeatedArmies, key);
            });


        logger.RunAndLogTime("Removing empty units", LogType.Logic,
        () =>
        {
            foreach (var army in Graph.NodesById.Values.OfType<Army>())
            {
                var empty = army.Units.Entities(key.Data)
                    .Where(u => u.Troops.Contents.Keys.All(k => k == 0));
                if (empty.Any())
                {
                    foreach (var unit in empty.ToArray())
                    {
                        GD.Print("removing unit");
                        key.Data.RemoveEntity(unit.Id, key);
                    }
                }
            }
        });
        
        
        void doFor<TType>(Action<TType> act)
        {
            foreach (var t in Graph.GetNodes().OfType<TType>())
            {
                act(t);
            }
        }
    }

    private void DistributeResources(LogicWriteKey key, Logger logger)
    {
        logger.RunAndLogTime("Distributing resources",
            LogType.Logic,
            () =>
            {
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
            });
    }

    private void PruneEmptyNodes(Logger logger)
    {
        logger.RunAndLogTime("Pruning empty combat nodes", LogType.Logic,
            () =>
            {
                var emptyAttack = Graph.NodesById
                    .Values.OfType<CellAttackNode>()
                    .Where(atk => atk.UnitInfos.Count == 0).ToArray();
                foreach (var atk in emptyAttack)
                {
                    Graph.RemoveNode(atk);
                }

                var emptyDefend = Graph.NodesById
                    .Values.OfType<CellDefenseNode>()
                    .Where(def => Graph.GetNeighbors(def).OfType<CellAttackNode>().Count() == 0)
                    .ToArray();
                foreach (var def in emptyDefend)
                {
                    Graph.RemoveNode(def);
                }

                var emptyArmy = Graph.NodesById.Values.OfType<Army>()
                    .Where(a => Graph.GetNeighbors(a).Any() == false).ToArray();
                foreach (var army in emptyArmy)
                {
                    Graph.RemoveNode(army);
                }
            });
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