
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.LinearSolver;

public static class BudgetConstrainer
{
    public static void SetItemConstraints<T>(this Solver solver, Data data, ItemCount budget,
        Dictionary<T, Variable> vars) where T : IModel
    {
        var items = data.Models.GetModels<Item>().Select(kvp => kvp.Value.Id).ToList();
        var itemNumConstraints = new Dictionary<int, Constraint>();
        items.ForEach(i =>
        {
            var itemConstraint = solver.MakeConstraint(0f, budget.Get(i));
            itemNumConstraints.Add(i, itemConstraint);
        });
        foreach (var kvp in vars)
        {
            var b = kvp.Key;
            var projVar = kvp.Value;
            var buildCosts = b.GetAttribute<MakeableAttribute>().ItemCosts;
            foreach (var kvp2 in buildCosts)
            {
                var item = kvp2.Key;
                var num = kvp2.Value;
                var itemConstraint = itemNumConstraints[item.Id];
                itemConstraint.SetCoefficient(projVar, num);
            }
        }
    }
    
    public static void SetCreditConstraint<T>(this Solver solver, Data data, float credit,
        Dictionary<Item, float> prices, 
        Dictionary<T, Variable> buildingVars) where T : IModel
    {
        var creditConstraint = solver.MakeConstraint(0f, credit, "Credits");
        foreach (var kvp in buildingVars)
        {
            var projVar = kvp.Value;
            var buildCosts = kvp.Key.GetAttribute<MakeableAttribute>().ItemCosts;
            var projPrice = buildCosts
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
    
}