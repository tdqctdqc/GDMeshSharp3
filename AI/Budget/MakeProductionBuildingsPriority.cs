
using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.LinearSolver;

public class MakeProductionBuildingsPriority
    : SettlementBuildingConstructionPriority
{
    public IModel Model { get; private set; }
    public BudgetBranch Parent { get; }

    public MakeProductionBuildingsPriority(IModel model, 
        Regime r, string name) 
        : base(r, name)
    {
        Model = model;
    }

    protected override float Utility(SettlementBuilding t, Data d)
    {
        return t.Labor.Outputs.Contents[Model.Id];
    }

    protected override bool Relevant(SettlementBuilding t, Data d)
    {
        if (t.Labor.Outputs.Contents.ContainsKey(Model.Id) == false)
        {
            return false;
        }

        return true;
    }

    
}