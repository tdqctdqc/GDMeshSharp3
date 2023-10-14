
using System;
using System.Linq;
using System.Collections.Generic;

public class ForceCompositionAi
{
    public float BuildTroopWeight { get; private set; }
    public IBudgetPriority[] Priorities { get; private set; }
    public ForceCompositionAi()
    {
        BuildTroopWeight = 1f;
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
        var templates = data.Military.UnitAux.UnitTemplates[regime];
        orders.MilitaryOrders.UnitTemplatesToBuild.Add(templates.First().Id);
    }
    
    private void DoPriorities(MajorTurnOrders orders, BudgetPool pool, Data data,
         Regime regime)
    {
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