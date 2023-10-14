using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.LinearSolver;


public class ReserveTroopBuildPriority : SolverPriority<Troop>
{
    //USE FOR RESERVE 
    public ReserveTroopBuildPriority(string name, 
        Func<Data, Regime, float> getWeight, 
        Func<Troop, bool> relevant, Func<Troop, float> utility) 
        : base(name, d => d.Models.GetModels<Troop>().Values, getWeight, relevant, utility)
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

    protected override void Complete(Regime r, MajorTurnOrders orders, 
        Dictionary<Troop, int> toBuild, Data data)
    {
        foreach (var kvp in toBuild)
        {
            var troop = kvp.Key;
            var num = kvp.Value;
            if (num == 0) continue;
            for (var i = 0; i < num; i++)
            {
                var buildCosts = troop.Makeable.ItemCosts.GetEnumerableModel(data);
                foreach (var cost in buildCosts)
                {
                    Account.UseItem(cost.Key, cost.Value);
                }
                var proj = new TroopManufactureProject(-1, 0f, num, troop.MakeRef());
                orders.ManufacturingOrders.ToStart.Add(proj);
            }
        }
    }
}