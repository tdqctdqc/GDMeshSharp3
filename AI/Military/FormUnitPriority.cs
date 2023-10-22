using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;

public class FormUnitPriority : SolverPriority<UnitTemplate>
{
    public FormUnitPriority(string name, Func<Data, IEnumerable<UnitTemplate>> getAll, Func<Data, Regime, float> getWeight, Func<UnitTemplate, bool> relevant, Func<UnitTemplate, float> utility) : base(name, getAll, getWeight, relevant, utility)
    {
    }

    protected override void SetConstraints(Solver solver, Regime r, 
        Dictionary<UnitTemplate, Variable> projVars, Data data)
    {
        solver.SetNumConstraints<UnitTemplate, Troop>(
            data.Models.GetModels<Troop>().Values.ToList(),
            t => t.TroopCounts.GetEnumerableModel(data)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            data, r.Military.TroopReserve, projVars);
    }

    protected override void SetWishlistConstraints(Solver solver, Regime r, 
        Dictionary<UnitTemplate, Variable> projVars, Data data, 
        BudgetPool pool, float proportion)
    {
        solver.SetNumConstraints<UnitTemplate, Troop>(
            data.Models.GetModels<Troop>().Values.ToList(),
            t => t.TroopCounts.GetEnumerableModel(data)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            data, r.Military.TroopReserve, projVars);
    }

    protected override void Complete(Regime r, MajorTurnOrders orders, 
        Dictionary<UnitTemplate, int> toBuild, Data data)
    {
        foreach (var kvp in toBuild)
        {
            for (var i = 0; i < kvp.Value; i++)
            {
                orders.MilitaryOrders.UnitTemplatesToForm.Add(kvp.Key.Id);
            }
        }
    }
}