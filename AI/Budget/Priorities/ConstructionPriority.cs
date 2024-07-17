
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;

public abstract class ConstructionPriority 
    : SolverPriority<SettlementBuilding>
{
    public ConstructionPriority(Regime r, string name) 
        : base(name, 
            d => d.Models.GetModels<SettlementBuilding>()
                .Where(b => r.HasPrereqs(b)))
    {
    }

    protected override void SetCalcData(Regime r, Data d)
    {
        
    }

    protected override void SetConstraints(Solver solver, 
        Regime r,
        BudgetPool pool,
        Dictionary<SettlementBuilding, Variable> projVars, Data data)
    {
        solver.SetBuildCostConstraints(data, pool, projVars);
        solver.SetMaintainCostConstraints(data, pool, projVars,
            b =>
            {
                return b.Labor.Inputs.GetEnumModel(data)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            });
        // solver.SetBuildingSlotConstraints(r, projVars, data);
        
        var laborConstraint = solver.MakeConstraint(0f,
            pool.Stock.Get(data.Models.Items.Labor));
        
        foreach (var (b, variable) in projVars)
        {
            laborConstraint.SetCoefficient(variable, b.Labor.TotalLabor());
        }
    }
    
    protected override Dictionary<IModel, float> GetCosts(
        Dictionary<SettlementBuilding, float> toBuild, 
        Data d)
    {
        var res = new Dictionary<IModel, float>();
        foreach (var (building, num) in toBuild)
        {
            foreach (var (id, amt) in building.Makeable.BuildCosts.Contents)
            {
                var model = d.Models.GetModel<IModel>(id);
                res.AddOrSum(model, amt * num);
            }
        }

        return res;
    }
}