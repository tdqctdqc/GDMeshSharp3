
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;

public static class BudgetConstrainer
{
    public static void SetMaxVariableConstraint<T>(
        this Solver solver,
        Dictionary<T, Variable> vars, Dictionary<T, float> maxes, Data data)
    {
        foreach (var (key, variable) in vars)
        {
            if (maxes.ContainsKey(key))
            {
                var constraint = Mathf.Min(variable.Ub(), maxes[key]);
                variable.SetUb(constraint);
            }
        }
    }


    public static void SetBuildCostConstraints<TBuild>(
            this Solver solver, 
            Data data, BudgetPool pool, 
            Dictionary<TBuild, Variable> vars
        ) where TBuild : IMakeable
    {
        var constraints = new Dictionary<int, Constraint>();
        foreach (var (build, variable) in vars)
        {
            var costs = build.Makeable
                .BuildCosts.GetEnumerableModel(data);
            foreach (var (model, amount) in costs)
            {
                if (constraints.TryGetValue(model.Id, 
                        out var constraint) == false)
                {
                    constraint = solver.MakeConstraint(0f,
                        pool.Stock.Get(model));
                }
                constraint.SetCoefficient(variable, amount);
            }
        }
    }
    
    public static void SetMaintainCostConstraints<TBuild>(
        this Solver solver, 
        Data data, BudgetPool pool, 
        Dictionary<TBuild, Variable> vars,
        Func<TBuild, Dictionary<IModel, float>?> getMaintainCosts)
            where TBuild : IMakeable
    {
        var constraints = new Dictionary<int, Constraint>();
        foreach (var (build, variable) in vars)
        {
            var costs = getMaintainCosts(build);
            if (costs is null) continue;
            foreach (var (model, amount) in costs)
            {
                if (constraints.TryGetValue(model.Id, 
                        out var constraint) == false)
                {
                    constraint = solver.MakeConstraint(0f,
                        pool.Net.Get(model));
                }
                constraint.SetCoefficient(variable, amount);
            }
        }
    }
    
    
    public static void SetCreditConstraint<T>(this Solver solver, Data data, float credit,
        Dictionary<IModel, float> prices, 
        Dictionary<T, Variable> vars) where T : IMakeable
    {
        var creditConstraint = solver.MakeConstraint(0f, credit, "Credits");
        foreach (var kvp in vars)
        {
            var projVar = kvp.Value;
            var buildCosts = kvp.Key.Makeable.BuildCosts.GetEnumerableModel(data);
            var projPrice = buildCosts
                .Where(kvp => kvp.Key is TradeableItem)
                .Sum(kvp => prices[kvp.Key] * kvp.Value);
            creditConstraint.SetCoefficient(projVar, projPrice);
        }
    }
}