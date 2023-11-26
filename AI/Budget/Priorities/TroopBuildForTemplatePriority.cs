using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;

public class TroopBuildForTemplatePriority : SolverPriority<UnitTemplate>
{
    public TroopBuildForTemplatePriority(string name, Regime regime, Func<Data, Regime, float> getWeight, 
        Func<UnitTemplate, bool> relevant, Func<UnitTemplate, float> utility) 
        : base(name, 
            d => regime.GetUnitTemplates(d),
            getWeight, relevant, utility)
    {
    }

    protected override void SetConstraints(Solver solver, Regime r, 
        Dictionary<UnitTemplate, Variable> projVars, Data data)
    {
        solver.SetIndustrialPointConstraints(r, data, 3f, projVars);
        solver.SetItemsConstraints(data, Account.Items, projVars);
    }

    protected override void SetWishlistConstraints(Solver solver, Regime r, Dictionary<UnitTemplate, Variable> projVars, Data data, BudgetPool pool, float proportion)
    {
        solver.SetItemConstraint(data.Models.Items.Recruits, data, Account.Items, projVars);
    }

    protected override void Complete(Regime r, MajorTurnOrders orders, 
        Dictionary<UnitTemplate, int> toBuild, LogicWriteKey key)
    {
        var allTroops = IdCount<Troop>.Construct(new Dictionary<Troop, float>());
        foreach (var kvp1 in toBuild)
        {
            var num = kvp1.Value;
            var troops = kvp1.Key.TroopCounts.GetEnumerableModel(key.Data);
            
            foreach (var kvp2 in troops)
            {
                allTroops.Add(kvp2.Key, kvp2.Value * num);
            }
        }
        foreach (var kvp in allTroops.GetEnumerableModel(key.Data))
        {
            foreach (var kvp2 in kvp.Key.Makeable.ItemCosts.GetEnumerableModel(key.Data))
            {
                Account.UseItem(kvp2.Key, kvp.Value * kvp2.Value);
            }
            var proj = new TroopManufactureProject(key.Data.IdDispenser.TakeId(), 0f, kvp.Value, kvp.Key.MakeRef());
            var proc = new StartManufacturingProjectProc(r.MakeRef(), proj);
            key.SendMessage(proc);
        }
    }
}