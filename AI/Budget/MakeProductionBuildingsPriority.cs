
using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.LinearSolver;

public abstract class MakeProductionBuildingsPriority
    : SettlementBuildingConstructionPriority
{
    public ModelRef<IModel> Model { get; private set; }

    public MakeProductionBuildingsPriority(ModelRef<IModel> model, string name) 
        : base(name)
    {
        Model = model;
    }

    protected override string GetName(SettlementBuilding t, Data d)
    {
        return t.Name;
    }

    protected override float Utility(SettlementBuilding t, Data d)
    {
        return t.Labor.Outputs.Contents[Model.RefId];
    }

    protected override bool Relevant(SettlementBuilding t, Data d)
    {
        return t.Labor.Outputs.Contents.ContainsKey(Model.RefId);
    }

    protected override IEnumerable<SettlementBuilding> GetAll(Regime r, Data d)
    {
        return d.Models.GetModels<SettlementBuilding>()
            .Where(t => t.Labor.Outputs.Contents.ContainsKey(Model.RefId));
    }


    protected override void Complete(BudgetPool pool, Regime r, Dictionary<SettlementBuilding, float> toBuild, LogicKey key)
    {
        CompleteModel(pool, r, toBuild, key);
    }
    
}