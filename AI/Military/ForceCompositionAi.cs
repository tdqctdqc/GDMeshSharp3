
using System;
using System.Linq;
using System.Collections.Generic;
using Godot;

public class ForceCompositionAi
{
    public float BuildTroopWeight { get; private set; }
    public IBudgetPriority[] Priorities { get; private set; }
    private static int PreferredGroupSize = 7;

    public ForceCompositionAi(Regime regime)
    {
        BuildTroopWeight = 1f;
        Priorities = new IBudgetPriority[]
        {
            new FormUnitPriority("Form unit",
                d => regime.GetUnitTemplates(d),
                (d,r) => 1f,
                t => true,
                t => 1f)
        };
    }

    public void Calculate(Regime regime, LogicWriteKey key, MajorTurnOrders orders,
         IdCount<Troop> reserve)
    {
        SetBuildTroopWeight(regime, key.Data);
        ReinforceUnits(reserve);
        BuildUnits(reserve, key, regime, orders);
        AssignFreeUnitsToGroups(regime, key, orders);
    }
    private void AssignFreeUnitsToGroups(Regime regime, LogicWriteKey key, MajorTurnOrders orders)
    {
        var freeUnits = key.Data.Military.UnitAux.UnitByRegime[regime]
            ?.Where(u => u != null)
            .Where(u => key.Data.Military.UnitAux.UnitByGroup[u] == null);
        if (freeUnits == null || freeUnits.Count() == 0) return;
        var numGroups = Mathf.CeilToInt((float)freeUnits.Count() / PreferredGroupSize);
        var newGroups = Enumerable.Range(0, numGroups)
            .Select(i => new List<int>())
            .ToList();
        
        var iter = 0;
        foreach (var freeUnit in freeUnits)
        {
            var group = iter % numGroups;
            key.Data.Logger.Log($"adding unit to group pre", LogType.Temp);

            newGroups.ElementAt(group).Add(freeUnit.Id);
            iter++;
        }
        foreach (var newGroup in newGroups)
        {
            key.Data.Logger.Log($"creating new group from {newGroup.Count()} units", LogType.Temp);

            UnitGroup.Create(orders.Regime.Entity(key.Data),
                newGroup, key);
        }
    }
    private void SetBuildTroopWeight(Regime regime, Data data)
    {
        BuildTroopWeight = 1f;
    }
    private void ReinforceUnits(IdCount<Troop> reserve)
    {
        
    }

    private void BuildUnits(IdCount<Troop> reserve, LogicWriteKey key, Regime regime, 
        MajorTurnOrders orders)
    {
        var pool = new BudgetPool(
            IdCount<Item>.Construct(),
            IdCount<IModel>.Construct<IModel, Troop>(regime.Military.TroopReserve), 
            0f);
        DoPriorities(orders, pool, key, regime);
    }
    
    private void DoPriorities(MajorTurnOrders orders, BudgetPool pool, LogicWriteKey key,
         Regime regime)
    {
        foreach (var bp in Priorities)
        {
            bp.SetWeight(key.Data, regime);
        }
        var totalPriority = Priorities.Sum(p => p.Weight);
        if (totalPriority <= 0f) throw new Exception();
        foreach (var priority in Priorities)
        {
            priority.Wipe();
            var proportion = priority.Weight / totalPriority;
            priority.SetWishlist(regime, key.Data, pool, proportion);
            priority.FirstRound(orders, regime, proportion, pool, key);
        }
        foreach (var priority in Priorities)
        {
            var proportion = priority.Weight / totalPriority;
            priority.SecondRound(orders, regime, proportion, pool, key, 3f);
        }
    }
}