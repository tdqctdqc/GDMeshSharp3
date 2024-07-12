
using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.LinearSolver;

public class MakeProductionBuildingsPriority
    : ConstructionPriority
{
    public IModel Model { get; private set; }
    public BudgetBranch Parent { get; }

    public MakeProductionBuildingsPriority(IModel model, string name) 
        : base(name)
    {
        Model = model;
    }

    protected override float Utility(SettlementBuilding t)
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