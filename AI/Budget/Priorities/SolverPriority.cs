using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;


public abstract class SolverPriority<TBuild> : IBudgetPriority
    where TBuild : class, IModel, IMakeable
{
    public string Name { get; private set; }
    protected Func<Data, IEnumerable<TBuild>> _getAll;
    
    public SolverPriority(string name, 
        Func<Data, IEnumerable<TBuild>> getAll) 
    {
        Name = name;
        _getAll = getAll;
    }

    Dictionary<IModel, float> IBudgetPriority.GetWishlist(Regime regime, Data d)
        => GetWishlist(regime, d)
            .ToDictionary(kvp => (IModel)kvp.Key,
                kvp => (float)kvp.Value);
    public Dictionary<TBuild, int> GetWishlist(
        Regime regime,
        Data d)
    {
        var all = _getAll(d);
        
        var expandedPool = BudgetPool.ConstructForRegime(regime, d);
        foreach (var i in expandedPool.Stock.Contents.Keys.ToList())
        {
            expandedPool.Stock.Contents[i] *= 10f;
        }
        SetCalcData(regime, d);
        var solver = MakeSolver();
        var projVars = MakeProjVars(solver, d);
        SetConstraints(solver, regime, expandedPool, projVars, d);
        var success = Solve(solver, projVars);
        
        return projVars
            .Where(v => v.Value.SolutionValue() > 0f)
            .ToDictionary(v => v.Key, v => (int)v.Value.SolutionValue());
    }
    public Dictionary<IModel, float> GetWishlistCosts(
        Regime regime, 
        Data d)
    {
        return GetCosts(GetWishlist(regime, d), d);
    }
    public bool Calculate(BudgetPool pool, 
        Regime regime, 
        LogicWriteKey key,
        out Dictionary<IModel, float> modelCosts)
    {
        SetCalcData(regime, key.Data);
        var solver = MakeSolver();
        var projVars = MakeProjVars(solver, key.Data);
        SetConstraints(solver, regime, pool, projVars, key.Data);
        
        var success = Solve(solver, projVars);
        var toBuild = projVars
            .Where(v => v.Value.SolutionValue() > 0f)
            .ToDictionary(v => v.Key, v => (int)v.Value.SolutionValue());

        if (success != Solver.ResultStatus.OPTIMAL
            && success != Solver.ResultStatus.FEASIBLE)
        {
            GD.Print($"{Name} { success.ToString()} count {toBuild.Sum(kvp => kvp.Value)}");
        }
        Complete(pool, regime, toBuild, key);
        modelCosts = GetCosts(toBuild, key.Data);
        return toBuild.Count > 0;
    }

    protected abstract float Utility(TBuild t);
    protected abstract bool Relevant(TBuild t, Data d);
    protected abstract void SetCalcData(Regime r, Data d);
    protected abstract void SetConstraints(Solver solver, 
        Regime r,
        BudgetPool pool,
        Dictionary<TBuild, Variable> projVars, 
        Data data);

    protected abstract Dictionary<IModel, float>
        GetCosts(Dictionary<TBuild, int> toBuild, Data d);
    
    private Solver.ResultStatus Solve(Solver solver, 
        Dictionary<TBuild, Variable> projVars)
    {
        var objective = solver.Objective();
        objective.SetMaximization();
        
        foreach (var kvp in projVars)
        {
            var b = kvp.Key;
            var projVar = projVars[b];
            var benefit = Utility(b);
            objective.SetCoefficient(projVar, benefit);
        }
        return solver.Solve();
    }
    
    protected Dictionary<TBuild, Variable> MakeProjVars(
        Solver solver, 
        Data data)
    {
        var models = _getAll(data)
            .Where(t => Relevant(t, data));
        return models.Select(b =>
        {
            var projVar = solver.MakeIntVar(0, 1000, b.Id.ToString());
            return new KeyValuePair<TBuild, Variable>(b, projVar);
        }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
    
    
    protected Solver MakeSolver()
    {
        var solver = Solver.CreateSolver("CBC_MIXED_INTEGER_PROGRAMMING");
        // var solver = Solver.CreateSolver("GLOP");
        if (solver is null)
        {
            throw new Exception("solver null");
        }

        return solver;
    }

    protected virtual void Complete(
        BudgetPool pool,
        Regime r,
        Dictionary<TBuild, int> toBuild,
        LogicWriteKey key)
    {
        foreach (var (model, value) in toBuild)
        {
            var make = ModelMakeProject.Construct(r, model, value);
            var proc = StartMakeProjectProc.Construct(r.MakeRef(), make, key);
            key.SendMessage(proc);
        }
    }
}