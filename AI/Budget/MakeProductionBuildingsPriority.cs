
using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.LinearSolver;

public class MakeProductionBuildingsPriority
    : ConstructionPriority
{
    public IModel Model { get; private set; }
    public BudgetBranch Parent { get; }

    public MakeProductionBuildingsPriority(IModel model,
        string name, 
        Func<Data, Regime, float> getWeight) 
        : base(name, getWeight)
    {
        Model = model;
    }

    protected override float Utility(BuildingModel t)
    {
        return t.GetComponent<BuildingProd>().Outputs.Contents[Model.Id];
    }

    protected override bool Relevant(BuildingModel t, Data d)
    {
        if (t.HasComponent<BuildingProd>() == false)
        {
            return false;
        }

        var prod = t.GetComponent<BuildingProd>();
        if (prod.Outputs.Contents.ContainsKey(Model.Id) == false)
        {
            return false;
        }

        return true;
    }

    protected override Dictionary<IModel, float> GetCosts(
        Dictionary<BuildingModel, int> toBuild, 
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