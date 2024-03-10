
using System;
using System.Collections.Generic;
using Google.OrTools.LinearSolver;

public class ReinforcementTroopBuildPriority : SolverPriority<Troop>
{
    private Dictionary<Troop, float> _needed;
    public ReinforcementTroopBuildPriority(string name, 
        Func<Data, Regime, float> getWeight) 
        : base(name, 
            d => d.Models.GetModels<Troop>().Values, 
            getWeight)
    {
        _needed = new Dictionary<Troop, float>();
    }

    protected override void SetCalcData(Regime r, Data d)
    {
        _needed.Clear();
        foreach (var unit in r.GetUnits(d))
        {
            var template = unit.Template.Entity(d);
            foreach (var (troop, count) in unit.Troops.GetEnumerableModel(d))
            {
                var shouldHave = template.TroopCounts.Get(troop);
                if (shouldHave > count)
                {
                    _needed.AddOrSum(troop, shouldHave - count);
                }
            }
        }
    }
    protected override float Utility(Troop t)
    {
        return t.GetPowerPoints();
    }

    protected override bool Relevant(Troop t, Data d)
    {
        return _needed.ContainsKey(t);
    }
    protected override void SetConstraints(Solver solver, Regime r, 
        Dictionary<Troop, Variable> projVars, Data data)
    {
        solver.SetIndustrialPointConstraints(r, data, 3f, projVars);
        solver.SetItemsConstraints(data, Account.Items, projVars);
        solver.SetMaxVariableConstraint(projVars, _needed, data);
    }

    protected override void SetWishlistConstraints(Solver solver, Regime r, Dictionary<Troop, Variable> projVars, Data data, BudgetPool pool, float proportion)
    {
        solver.SetItemConstraint(data.Models.Items.Recruits, data, 
            Account.Items, projVars);
    }

    protected override void Complete(Regime r, 
        Dictionary<Troop, int> toBuild, LogicWriteKey key)
    {
        foreach (var kvp in toBuild)
        {
            var troop = kvp.Key;
            var num = kvp.Value;
            if (num == 0) continue;
            for (var i = 0; i < num; i++)
            {
                var proj = new TroopManufactureProject(-1, 0f, num, troop.MakeRef());
                var proc = new StartManufacturingProjectProc(r.MakeRef(), proj);
                key.SendMessage(proc);
            }
        }
    }
}