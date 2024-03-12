
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;

public abstract class ConstructionPriority 
    : SolverPriority<BuildingModel>
{
    public ConstructionPriority(string name, 
        Func<Data, Regime, float> getWeight) 
        : base(name, 
            d => d.Models.GetModels<BuildingModel>().Values, 
            getWeight)
    {
    }



    protected override void SetCalcData(Regime r, Data d)
    {
        
    }

    protected override void SetConstraints(Solver solver, 
        Regime r,
        BudgetPool pool,
        Dictionary<BuildingModel, Variable> projVars, Data data)
    {
        solver.SetModelConstraints(data, pool, projVars);
        solver.SetBuildingSlotConstraints(r, projVars, data);
    }

    protected override void Complete(
        BudgetPool pool,
        Regime r, 
        Dictionary<BuildingModel, int> toBuild, 
        LogicWriteKey key)
    {
        foreach (var (model, value) in toBuild)
        {
            var make = MakeProject.Construct(model, value);
            var proc = new StartMakeProjectProc(r.MakeRef(), make);
            key.SendMessage(proc);
        }
    }
}