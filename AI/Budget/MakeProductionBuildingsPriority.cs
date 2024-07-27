
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

    protected override string GetName(SettlementBuilding t, Data d)
    {
        return t.Name;
    }

    protected override float Utility(SettlementBuilding t, Data d)
    {
        return t.Labor.Outputs.Contents[Model.Id];
    }

    protected override bool Relevant(SettlementBuilding t, Data d)
    {
        return t.Labor.Outputs.Contents.ContainsKey(Model.Id);
    }

    protected override IEnumerable<SettlementBuilding> GetAll(Data d)
    {
        return d.Models.GetModels<SettlementBuilding>()
            .Where(t => t.Labor.Outputs.Contents.ContainsKey(Model.Id));
    }


    protected override void Complete(BudgetPool pool, Regime r, Dictionary<SettlementBuilding, float> toBuild, LogicWriteKey key)
    {
        CompleteModel(pool, r, toBuild, key);
    }
}