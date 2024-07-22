
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.LinearSolver;

public class ResourceExtractionConstructionPriority
    : SolverPriority<ResourceExtractionBuilding>
{
    public IModel Model { get; private set; }
    public BudgetBranch Parent { get; }

    public ResourceExtractionConstructionPriority(IModel model, 
        Regime r, string name) 
        : base(name, d => d.Models.GetModels<ResourceExtractionBuilding>()
                .Where(b => b.Resource(d) == model
                    && r.HasPrereqs(b)))
    {
        Model = model;
    }

    protected override float Utility(ResourceExtractionBuilding t, Data d)
    {
        return t.Labor.Outputs.Contents[Model.Id];
    }

    protected override bool Relevant(ResourceExtractionBuilding t, Data d)
    {
        if (t.Labor.Outputs.Contents.ContainsKey(Model.Id) == false)
        {
            return false;
        }

        return true;
    }
    protected override void SetCalcData(Regime r, Data d)
    {
    }

    protected override void SetConstraints(Solver solver, 
        Regime r,
        BudgetPool pool,
        Dictionary<ResourceExtractionBuilding, Variable> projVars,
        Data data)
    {
        var deposits = r.GetCells(data).Sum(c =>
        {
            if (c.HasResourceDeposit(data) == false) return 0;
            var rd = c.GetResourceDeposit(data);
            if (rd.Item.RefId != Model.Id) return 0;
            if (rd.Extraction.Fulfilled()) return 0;
            return 1;
        });

        var constraint = solver.MakeConstraint(0f, deposits);
        foreach (var (xb, variable) in projVars)
        {
            constraint.SetCoefficient(variable, 1);
        }
        
        solver.SetBuildCostConstraints(data, pool, projVars);
        solver.SetMaintainCostConstraints(data, pool, projVars,
            b =>
            {
                return b.Labor.Inputs.GetEnumModel(data)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            });
        
        var laborConstraint = solver.MakeConstraint(0f,
            pool.Stock.Get(data.Models.Items.Labor));
        
        foreach (var (b, variable) in projVars)
        {
            laborConstraint.SetCoefficient(variable, b.Labor.TotalLabor());
        }
    }
    
    protected override Dictionary<IModel, float> GetCosts(
        Dictionary<ResourceExtractionBuilding, float> toBuild, 
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