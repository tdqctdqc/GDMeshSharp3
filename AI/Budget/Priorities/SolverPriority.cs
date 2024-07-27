using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;


public abstract class SolverPriority<TBuild> : IBudgetPriority
    where TBuild : class, 
    IIdentifiable
{
    public string Name { get; private set; }
    
    public SolverPriority(string name) 
    {
        Name = name;
    }

    Dictionary<IModel, float> IBudgetPriority.GetWishlist(Regime regime, Data d)
        => GetWishlist(regime, d)
            .ToDictionary(kvp => (IModel)kvp.Key,
                kvp => (float)kvp.Value);
    public Dictionary<TBuild, float> GetWishlist(
        Regime regime,
        Data d)
    {
        var all = GetAll(d);
        
        var expandedPool = BudgetPool.ConstructForRegime(regime, d);
        foreach (var i in expandedPool.Stock.Contents.Keys.ToList())
        {
            expandedPool.Stock.Contents[i] *= 10f;
        }
        SetCalcData(regime, d);
        var solver = MakeSolver();
        var projVars = MakeProjVars(solver, d);
        SetConstraints(solver, regime, expandedPool, projVars, d);
        var success = Solve(solver, projVars, d);
        
        return projVars
            .Where(v => v.Value.SolutionValue() > 0f)
            .ToDictionary(v => v.Key, 
                v => (float)v.Value.SolutionValue());
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
        out Dictionary<IModel, float> modelCosts,
        out Dictionary<string, float> built)
    {
        SetCalcData(regime, key.Data);
        var solver = MakeSolver();
        var projVars = MakeProjVars(solver, key.Data);
        SetConstraints(solver, regime, pool, projVars, key.Data);
        
        var success = Solve(solver, projVars, key.Data);
        var toBuild = projVars
            .Where(v => v.Value.SolutionValue() > 0f)
            .ToDictionary(v => v.Key, 
                v => (float)v.Value.SolutionValue());

        if (success != Solver.ResultStatus.OPTIMAL
            && success != Solver.ResultStatus.FEASIBLE)
        {
            GD.Print($"{Name} { success.ToString()} count {toBuild.Sum(kvp => kvp.Value)}");
        }
        Complete(pool, regime, toBuild, key);
        modelCosts = GetCosts(toBuild, key.Data);
        built = toBuild.ToDictionary(kvp => GetName(kvp.Key, key.Data),
            kvp => kvp.Value);
        return toBuild.Count > 0;
    }

    protected abstract string GetName(TBuild t, Data d);
    protected abstract float Utility(TBuild t, Data d);
    protected abstract bool Relevant(TBuild t, Data d);
    protected abstract void SetCalcData(Regime r, Data d);
    protected abstract void SetConstraints(Solver solver, 
        Regime r,
        BudgetPool pool,
        Dictionary<TBuild, Variable> projVars, 
        Data data);

    protected abstract Dictionary<IModel, float>
        GetCosts(Dictionary<TBuild, float> toBuild, Data d);

    protected abstract IEnumerable<TBuild> GetAll(Data d);
    private Solver.ResultStatus Solve(Solver solver, 
        Dictionary<TBuild, Variable> projVars, Data d)
    {
        var objective = solver.Objective();
        objective.SetMaximization();
        
        foreach (var kvp in projVars)
        {
            var b = kvp.Key;
            var projVar = projVars[b];
            var benefit = Utility(b, d);
            objective.SetCoefficient(projVar, benefit);
        }
        return solver.Solve();
    }
    
    protected Dictionary<TBuild, Variable> MakeProjVars(
        Solver solver, 
        Data data)
    {
        var models = GetAll(data)
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

    protected abstract void Complete(
        BudgetPool pool,
        Regime r,
        Dictionary<TBuild, float> toBuild,
        LogicWriteKey key);
    
    
    protected void CompleteModel<TModel>
        (BudgetPool pool,
        Regime r,
        Dictionary<TModel, float> toBuild,
        LogicWriteKey key)
            where TModel : class, IModel, IMakeable
    {
        foreach (var (model, value) in toBuild)
        {
            var make = ModelMakeProject.Construct(r, model, value);
            var proc = new StartOrConsolidateMakeProject(make);
            key.SendMessage(proc);
        }
    }
}