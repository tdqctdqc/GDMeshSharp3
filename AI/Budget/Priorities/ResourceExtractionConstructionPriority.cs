
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.LinearSolver;

public class ResourceExtractionConstructionPriority
    : SolverPriority<ResourceExtractionBuilding>
{
    public ModelRef<IModel> Model { get; private set; }
    public ERef<Regime> Regime { get; private set; }
    public ResourceExtractionConstructionPriority(ModelRef<IModel> model, 
        ERef<Regime> regime, string name) : base(name)
    {
        Model = model;
        Regime = regime;
    }

    protected override string GetName(ResourceExtractionBuilding t, Data d)
    {
        return t.Name;
    }

    protected override float Utility(ResourceExtractionBuilding t, Data d)
    {
        return t.Labor.Outputs.Contents[Model.RefId];
    }

    protected override bool Relevant(ResourceExtractionBuilding t, Data d)
    {
        if (t.Labor.Outputs.Contents.ContainsKey(Model.RefId) == false)
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
            if (rd.Item.RefId != Model.RefId) return 0;
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

    protected override IEnumerable<ResourceExtractionBuilding> GetAll(Data d)
    {
        return d.Models.GetModels<ResourceExtractionBuilding>()
            .Where(b => b.Resource(d).Id == Model.RefId
                        && Regime.Get(d).HasPrereqs(b));
    }

    protected override void Complete(BudgetPool pool, Regime r, Dictionary<ResourceExtractionBuilding, float> toBuild, LogicKey key)
    {
        CompleteModel(pool, r, toBuild, key);
    }
}