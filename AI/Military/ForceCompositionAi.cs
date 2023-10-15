
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

    public void Calculate(Regime regime, Data data, MajorTurnOrders orders,
         IdCount<Troop> reserve)
    {
        SetBuildTroopWeight(regime, data);
        ReinforceUnits(reserve);
        BuildUnits(reserve, data, regime, orders);
    }

    private void SetBuildTroopWeight(Regime regime, Data data)
    {
        BuildTroopWeight = 1f;
    }
    private void ReinforceUnits(IdCount<Troop> reserve)
    {
        
    }

    private void BuildUnits(IdCount<Troop> reserve, Data data, Regime regime, 
        MajorTurnOrders orders)
    {
        var pool = new BudgetPool(
            IdCount<Item>.Construct(),
            IdCount<IModel>.Construct<IModel, Troop>(regime.TroopReserve), 
            0f);
        DoPriorities(orders, pool, data, regime);
    }
    
    private void DoPriorities(MajorTurnOrders orders, BudgetPool pool, Data data,
         Regime regime)
    {
        foreach (var bp in Priorities)
        {
            bp.SetWeight(data, regime);
        }
        var totalPriority = Priorities.Sum(p => p.Weight);
        if (totalPriority <= 0f) throw new Exception();
        foreach (var priority in Priorities)
        {
            priority.Wipe();
            var proportion = priority.Weight / totalPriority;
            priority.SetWishlist(regime, data, pool, proportion);
            priority.FirstRound(orders, regime, proportion, pool, data);
        }
        foreach (var priority in Priorities)
        {
            var proportion = priority.Weight / totalPriority;
            priority.SecondRound(orders, regime, proportion, pool, data, 3f);
        }
    }
}