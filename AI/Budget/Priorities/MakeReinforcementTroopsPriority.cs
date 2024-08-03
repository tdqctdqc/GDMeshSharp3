
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;

public class MakeReinforcementTroopsPriority
    : SolverPriority<Troop>
{
    public Dictionary<ModelRef<TroopType>, float> _needed { get; private set; }


    public MakeReinforcementTroopsPriority(string name) 
        : base(name)
    {
        _needed = new Dictionary<ModelRef<TroopType>, float>();
    }


    protected override string GetName(Troop t, Data d)
    {
        return t.Name;
    }

    protected override float Utility(Troop t, Data d)
    {
        return t.GetPowerPoints();
    }

    protected override bool Relevant(Troop t, Data d)
    {
        return _needed.ContainsKey(t.TroopType.MakeRef());
    }

    protected override void SetCalcData(Regime r, Data d)
    {
        _needed.Clear();
        var units = r.GetUnits(d);
        if(units is null) return;
        foreach (var unit in units)
        {
            var template = unit.Template.Get(d);
            foreach (var (troopType, amt) in unit.Troops
                         .GetEnumModel(d).SortInto(kvp => kvp.Key.TroopType, kvp => kvp.Value))
            {
                var diff = template.Troops.Get(troopType) - amt;
                var stock = r.Stock.Stock.Get(troopType);
                diff -= stock;
                var producing = r.MakeQueue.Queue.OfType<ModelMakeProject>()
                    .Where(p => p.Model.Get(d) == troopType)
                    .Sum(p => p.Amount);
                diff -= producing;
                if (diff > 0f)
                {
                    _needed.AddOrSum(troopType.MakeRef(), diff);
                }
            }
        }
    }

    protected override void SetConstraints(Solver solver, 
        Regime r, BudgetPool pool, 
        Dictionary<Troop, Variable> projVars, Data data)
    {
        solver.SetBuildCostConstraints(data, pool, projVars);
        solver.SetConstraints(projVars, t => (t.TroopType.MakeRef(), 1f),
            _needed, data);
    }

    protected override Dictionary<IModel, float> GetCosts
        (Dictionary<Troop, float> toBuild, Data d)
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

    protected override IEnumerable<Troop> GetAll(Regime r, Data d)
    {
        return d.Models.GetModels<Troop>()
            .Where(t => r.HasPrereqs(t));
    }

    protected override void Complete(BudgetPool pool, Regime r, Dictionary<Troop, float> toBuild, LogicKey key)
    {
        CompleteModel(pool, r, toBuild, key);
    }

    public override float GetWeight(Regime r, Data d)
    {
        var units = r.GetUnits(d);
        var str = units.Sum(u => u.GetPowerPoints(d));
        var authorized = units.Sum(u => u.Template.Get(d).Troops.GetEnumModel(d)
            .Sum(kvp => 
                r.Military.GetBestTroopOfType(kvp.Key, d)
                    .GetPowerPoints() * kvp.Value));
        if (authorized == 0f) return 0f;
        return 3f * (1f - str / authorized);
    }
}