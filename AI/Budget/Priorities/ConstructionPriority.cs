
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;

public abstract class ConstructionPriority 
    : SolverPriority<SettlementBuildingModel>
{
    public ConstructionPriority(string name) 
        : base(name, 
            d => d.Models.GetModels<SettlementBuildingModel>().Values)
    {
    }

    protected override void SetCalcData(Regime r, Data d)
    {
        
    }

    protected override void SetConstraints(Solver solver, 
        Regime r,
        BudgetPool pool,
        Dictionary<SettlementBuildingModel, Variable> projVars, Data data)
    {
        solver.SetBuildCostConstraints(data, pool, projVars);
        solver.SetMaintainCostConstraints(data, pool, projVars,
            b =>
            {
                if (b.GetComponent<LaborComponent>() is LaborComponent l)
                {
                    return l.Inputs.GetEnumModel(data)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }

                return null;
            });
        // solver.SetBuildingSlotConstraints(r, projVars, data);
        
        var laborConstraint = solver.MakeConstraint(0f,
            pool.Stock.Get(data.Models.Flows.Labor));
        
        foreach (var (b, variable) in projVars)
        {
            if (b.GetComponent<LaborComponent>() is LaborComponent l)
            {
                laborConstraint.SetCoefficient(variable, l.TotalLabor());
                
            }
        }
    }
}