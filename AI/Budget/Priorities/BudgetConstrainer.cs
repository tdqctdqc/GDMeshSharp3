
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;

public static class BudgetConstrainer
{
    public static void SetMaxVariableConstraint<T>(
        this Solver solver,
        Dictionary<T, Variable> vars, Dictionary<T, float> maxes, Data data)
    {
        foreach (var (key, variable) in vars)
        {
            if (maxes.ContainsKey(key))
            {
                var constraint = Mathf.Min(variable.Ub(), maxes[key]);
                variable.SetUb(constraint);
            }
        }
    }
    
    public static void SetItemsConstraints<T>(this Solver solver, Data data, 
        IdCount<Item> budget, Dictionary<T, Variable> vars)
        where T : IMakeable
    {
        solver.SetNumConstraints<T, Item>(
            data.Models.GetModels<Item>().Values.ToList(),
            t => t.Makeable.ItemCosts.GetEnumerableModel(data)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            data, budget, vars);
    }
    
    public static void SetNumConstraints<T, TConstrained>(this Solver solver, 
        List<TConstrained> constrained,
        Func<T, Dictionary<TConstrained, float>> getCosts,
        Data data, IdCount<TConstrained> budget, 
        Dictionary<T, Variable> vars)
            where T : IMakeable 
            where TConstrained : IIdentifiable
    {
        var constraints = new Dictionary<int, Constraint>();
        constrained.ForEach(c =>
        {
            var itemConstraint = solver.MakeConstraint(0f, budget.Get(c));
            constraints.Add(c.Id, itemConstraint);
        });
        foreach (var kvp in vars)
        {
            var b = kvp.Key;
            var projVar = kvp.Value;
            var buildCosts = getCosts(b);
            foreach (var kvp2 in buildCosts)
            {
                var item = kvp2.Key;
                var num = kvp2.Value;
                var itemConstraint = constraints[item.Id];
                itemConstraint.SetCoefficient(projVar, num);
            }
        }
    }
    
    
    public static void SetIndustrialPointConstraints<T>(this Solver solver, 
        Regime r, Data data, float maxBacklogRatio, 
        Dictionary<T, Variable> vars) 
        where T : IMakeable
    {
        var ipFlow = data.Models.Flows.IndustrialPower;
        var backlogRatio = 3f;
        var ipUsed = r.ManufacturingQueue.Queue
            .Sum(m => m.Remaining(data));
        var ipAvail = r.Flows.Get(ipFlow).Net() * backlogRatio - ipUsed;
        var ipConstraint = solver.MakeConstraint(0f, ipAvail);

        foreach (var kvp in vars)
        {
            var b = kvp.Key;
            var projVar = kvp.Value;
            var industrialCost = b.Makeable.IndustrialCost;
            ipConstraint.SetCoefficient(projVar, industrialCost);
        }
    }
    public static void SetItemConstraint<T>(this Solver solver, Item constrainedItem,
        Data data, IdCount<Item> budget, Dictionary<T, Variable> vars) 
        where T : IMakeable
    {
        var itemConstraint = solver.MakeConstraint(0f, budget.Get(constrainedItem));
        foreach (var kvp in vars)
        {
            var b = kvp.Key;
            var projVar = kvp.Value;
            var buildCosts = b.Makeable.ItemCosts.GetEnumerableModel(data);
            foreach (var kvp2 in buildCosts)
            {
                var item = kvp2.Key;
                var num = kvp2.Value;
                itemConstraint.SetCoefficient(projVar, num);
            }
        }
    }
    
    
    public static void SetCreditConstraint<T>(this Solver solver, Data data, float credit,
        Dictionary<Item, float> prices, 
        Dictionary<T, Variable> vars) where T : IMakeable
    {
        var creditConstraint = solver.MakeConstraint(0f, credit, "Credits");
        foreach (var kvp in vars)
        {
            var projVar = kvp.Value;
            var buildCosts = kvp.Key.Makeable.ItemCosts.GetEnumerableModel(data);
            var projPrice = buildCosts
                .Where(kvp => kvp.Key is TradeableItem)
                .Sum(kvp => prices[kvp.Key] * kvp.Value);
            creditConstraint.SetCoefficient(projVar, projPrice);
        }
    }
    
    
    public static void SetConstructCapConstraint(this Solver solver, 
        int availConstructCap, 
        Dictionary<BuildingModel, Variable> buildingVars)
    {
        var constructLaborConstraint = solver.MakeConstraint(0, availConstructCap, "ConstructLabor");
        foreach (var kvp in buildingVars)
        {
            var projVar = kvp.Value;
            var b = kvp.Key;
            constructLaborConstraint.SetCoefficient(projVar, b.ConstructionCapPerTick);
        }
    }
    
    
    public static void SetBuildingLaborConstraint(this Solver solver, int laborAvail, 
        Dictionary<BuildingModel, Variable> buildingVars)
    {
        var buildingLaborConstraint = solver.MakeConstraint(0, laborAvail, "BuildingLabor");
        foreach (var kvp in buildingVars)
        {
            var projVar = kvp.Value;
            var b = kvp.Key;
            if(b.HasComponent<Workplace>())
            {
                buildingLaborConstraint.SetCoefficient(projVar, b.GetComponent<Workplace>().TotalLaborReq());
            }
        }
    }
    public static void SetBuildingSlotConstraints(this Solver solver, 
        Regime regime, Dictionary<BuildingModel, Variable> buildingVars, Data data)
    {
        var slotConstraints = new Dictionary<BuildingType, Constraint>();
        var slotTypes = buildingVars.Select(kvp => kvp.Key.BuildingType).Distinct();
        
        foreach (var slotType in slotTypes)
        {
            var slots = regime.GetPolys(data).Select(p => p.PolyBuildingSlots[slotType]).Sum();
            var slotConstraint = solver.MakeConstraint(0, slots, slotType.ToString());
            slotConstraints.Add(slotType, slotConstraint);
        }
        foreach (var kvp in buildingVars)
        {
            var b = kvp.Key;
            var projVar = kvp.Value;
            var slotConstraint = slotConstraints[b.BuildingType];
            slotConstraint.SetCoefficient(projVar, 1);
        }
    }
}