
using System;
using System.Linq;
using System.Collections.Generic;

public class ForceCompositionAi
{
    public float BuildTroopWeight { get; private set; }
    public IBudgetPriority[] Priorities { get; private set; }
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