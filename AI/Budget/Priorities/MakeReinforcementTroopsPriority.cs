
using System;
using System.Collections.Generic;
using Google.OrTools.LinearSolver;

public class MakeReinforcementTroopsPriority
    : SolverPriority<Troop>
{
    private Dictionary<Troop, float> _needed; 
    public MakeReinforcementTroopsPriority(
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
        foreach (var unit in units)
        {
            var template = unit.Template.Get(d);
            foreach (var (troop, amt) in unit.Troops.GetEnumerableModel(d))
            {
                var diff = amt - template.TroopCounts.Get(troop);
                if (diff > 0f)
                {
                    _needed.AddOrSum(troop, diff);
                }
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
            foreach (var (model, modelAmt) in cost.GetEnumerableModel(d))
            {
                res.AddOrSum(model, modelAmt * amt);
            }
        }

        return res;
    }
}