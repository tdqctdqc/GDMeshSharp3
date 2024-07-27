
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;

public static class BudgetConstrainer
{
    private static float _flowMinCostProportion = .2f; 
    public static void SetVariableCaps<T>(
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
    
    public static void SetConstraints<T, TConstraint>(
        this Solver solver,
        Dictionary<T, Variable> vars, 
        Func<T, (TConstraint, float)> getConstraint,
        Dictionary<TConstraint, float> maxes, Data data)
    {
        var constraints = new Dictionary<TConstraint, Constraint>();
        foreach (var (constraintKey, constraintAmt) in maxes)
        {
            constraints.Add(constraintKey, solver.MakeConstraint(0f, constraintAmt));
        }

        foreach (var (key, variable) in vars)
        {
            var (constraintKey, constraintAmt) = getConstraint(key);
            
            if (maxes.ContainsKey(constraintKey))
            {
                var constraint = constraints[constraintKey];
                constraint.SetCoefficient(variable, constraintAmt);
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
                .BuildCosts.GetEnumModel(data);
            foreach (var (model, amount) in costs)
            {
                if (model is Flow f)
                {
                    var net = Mathf.Max(0f, pool.Net.Get(f));
                    if (constraints.TryGetValue(model.Id, 
                            out var constraint) == false)
                    {
                        constraint = solver.MakeConstraint(0f,
                            net);
                    }
                    constraint.SetCoefficient(variable, amount * _flowMinCostProportion);   
                }
                else
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
    }
    public static void SetBuildCostConstraints<TBuild>(
        this Solver solver, 
        Data data, BudgetPool pool, 
        Dictionary<TBuild, Variable> vars,
        Func<TBuild, MakeableAttribute> getMakeable)
    {
        var constraints = new Dictionary<int, Constraint>();
        foreach (var (build, variable) in vars)
        {
            var costs = getMakeable(build)
                .BuildCosts.GetEnumModel(data);
            foreach (var (model, amount) in costs)
            {
                if (model is Flow f)
                {
                    var net = Mathf.Max(0f, pool.Net.Get(f));
                    if (constraints.TryGetValue(model.Id, 
                            out var constraint) == false)
                    {
                        constraint = solver.MakeConstraint(0f,
                            net);
                    }
                    constraint.SetCoefficient(variable, amount * _flowMinCostProportion);   
                }
                else
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
    }
    
    public static void SetMaintainCostConstraints<TBuild>(
        this Solver solver, 
        Data data, BudgetPool pool, 
        Dictionary<TBuild, Variable> vars,
        Func<TBuild, Dictionary<Item, float>?> getMaintainCosts)
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
            var buildCosts = kvp.Key.Makeable.BuildCosts.GetEnumModel(data);
            var projPrice = buildCosts
                .Where(kvp => kvp.Key is TradeableItem)
                .Sum(kvp => prices[kvp.Key] * kvp.Value);
            creditConstraint.SetCoefficient(projVar, projPrice);
        }
    }
}