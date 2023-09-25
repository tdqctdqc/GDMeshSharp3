using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;


public abstract class BuildPriority<TBuild> : BudgetPriority
    where TBuild : IModel
{
    private Func<TBuild, bool> _relevant;
    private Func<TBuild, float> _utility;
    public BuildPriority(string name, 
        Func<Data, Regime, float> getWeight,
        Func<TBuild, bool> relevant,
        Func<TBuild, float> utility) 
        : base(name, getWeight)
    {
        _relevant = relevant;
        _utility = utility;
    }
    
    public override void Calculate(Regime regime, Data data, MajorTurnOrders orders)
    {
        var solver = MakeSolver();
        var projVars = MakeProjVars(solver, data);
        SetConstraints(solver, projVars, data);
        
        var success = Solve(solver, projVars);
        if (success == false)
        {
            foreach (var kvp in Account.Items.Contents)
            {
                var item = (Item) data.Models[kvp.Key];
                var q = kvp.Value;
            }
        }
        
        var toBuild = projVars.ToDictionary(v => v.Key, v => (int)v.Value.SolutionValue());
        Complete(regime, orders, toBuild, data);
    }

    protected abstract void SetConstraints(Solver solver, 
        Dictionary<TBuild, Variable> projVars, 
        Data data);
    protected abstract void SetWishlistConstraints(Solver solver,
        Dictionary<TBuild, Variable> projVars, 
        Data data, BudgetPool pool, float proportion);
    protected abstract void Complete(Regime r, MajorTurnOrders orders,
        Dictionary<TBuild, int> toBuild, Data data);
    private bool Solve(Solver solver, Dictionary<TBuild, Variable> projVars)
    {
        var objective = solver.Objective();
        objective.SetMaximization();
        
        foreach (var kvp in projVars)
        {
            var b = kvp.Key;
            var projVar = projVars[b];
            var benefit = _utility(b);
            objective.SetCoefficient(projVar, benefit);
        }
        var status = solver.Solve();
        return status == Solver.ResultStatus.OPTIMAL || status == Solver.ResultStatus.FEASIBLE;
    }
    public Dictionary<Item, int> CalculateWishlist(Regime regime, Data data,
        BudgetPool pool, float proportion)
    {
        var solver = MakeSolver();
        var projVars = MakeProjVars(solver, data);
        SetWishlistConstraints(solver, projVars, data, pool, proportion);
        
        var success = Solve(solver, projVars);
        if (success == false)
        {
            foreach (var kvp in Account.Items.Contents)
            {
                var item = (Item) data.Models[kvp.Key];
                var q = kvp.Value;
            }
        }
        
        var toBuild = projVars.ToDictionary(v => v.Key, v => (int)v.Value.SolutionValue());
        return projVars.GetCounts(
            kvp => kvp.Key.GetAttribute<MakeableAttribute>().ItemCosts, 
            (kvp, i) => Mathf.CeilToInt(i * kvp.Value.SolutionValue()));
    }
    
    private Dictionary<TBuild, Variable> MakeProjVars(Solver solver, Data data)
    {
        var models = data.Models.GetModels<TBuild>()
            .Values.Where(_relevant);
        return models.Select(b =>
        {
            var projVar = solver.MakeIntVar(0, 1000, b.Name);
            return new KeyValuePair<TBuild, Variable>(b, projVar);
        }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}