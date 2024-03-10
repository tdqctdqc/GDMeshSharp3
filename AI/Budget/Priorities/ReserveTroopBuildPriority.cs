using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.LinearSolver;

public class ReserveTroopBuildPriority : SolverPriority<Troop>
{
    public ReserveTroopBuildPriority(string name, 
        Func<Data, Regime, float> getWeight) 
        : base(name, d => d.Models.GetModels<Troop>().Values, getWeight)
    {
    }

    protected override float Utility(Troop t)
    {
        return t.GetPowerPoints();
    }

    protected override bool Relevant(Troop t, Data d)
    {
        return true;
    }

    protected override void SetCalcData(Regime r, Data d)
    {
        
    }

    protected override void SetConstraints(Solver solver, Regime r, 
        Dictionary<Troop, Variable> projVars, Data data)
    {
        solver.SetIndustrialPointConstraints(r, data, 3f, projVars);
        solver.SetItemsConstraints(data, Account.Items, projVars);
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