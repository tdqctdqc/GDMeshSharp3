

using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;

public class MakeReserveTroopsPriority : SolverPriority<Troop>
{
    private Dictionary<Troop, float> _needed; 
    public MakeReserveTroopsPriority(
        string name) 
            : base(name, d => d.Models.Troops.GetList())
    {
        _needed = new Dictionary<Troop, float>();
    }

    protected override float Utility(Troop t)
    {
        return t.GetPowerPoints();
    }

    protected override bool Relevant(Troop t, Data d)
    {
        return _needed.ContainsKey(t);
    }

    protected override void SetCalcData(Regime r, Data d)
    {
        _needed.Clear();
        var units = r.GetUnits(d);
        var desired = IdCount<Troop>.Construct();
        var reserveRatio = .2f;
        var templateCounts = IdCount<UnitTemplate>.Construct();
        foreach (var unit in units)
        {
            templateCounts.Add(unit.Template.RefId, 1);
        }
        foreach (var (template, num) in templateCounts
                     .GetEnumEntity(d))
        {
            foreach (var (troop, amt) in template.TroopCounts.GetEnumModel(d))
            {
                desired.Add(troop, amt * num * reserveRatio);
            }
        }
        
        foreach (var (troop, amt) in desired.GetEnumModel(d))
        {
            var stock = r.Stock.Stock.Get(troop);
            var diff = amt - stock;
            
            //'double counting' this w/ reinforcement priority?
            var producing = r.MakeQueue.Queue.OfType<ModelMakeProject>()
                .Where(p => p.Making.RefId == troop.Id)
                .Sum(p => p.Amount);
            diff -= producing;
            if (diff > 0f)
            {
                _needed.AddOrSum(troop, diff);
            }
        }
    }

    protected override void SetConstraints(Solver solver, 
        Regime r, BudgetPool pool, 
        Dictionary<Troop, Variable> projVars, Data data)
    {
        solver.SetBuildCostConstraints(data, pool, projVars);
        solver.SetMaxVariableConstraint(projVars,
            _needed, data);
    }

    protected override Dictionary<IModel, float> GetCosts
        (Dictionary<Troop, int> toBuild, Data d)
    {
        var res = new Dictionary<IModel, float>();

        foreach (var (troop, amt) in toBuild)
        {
            var cost = troop.Makeable.BuildCosts;
            foreach (var (model, modelAmt) in cost.GetEnumModel(d))
            {
                res.AddOrSum(model, modelAmt * amt);
            }
        }

        return res;
    }
}