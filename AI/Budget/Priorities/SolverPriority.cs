using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;


public abstract class SolverPriority<TBuild> : IBudgetPriority
    where TBuild : class, IModel, IMakeable
{
    public string Name { get; private set; }
    private Func<Data, Regime, float> _getWeight;
    private Func<Data, IEnumerable<TBuild>> _getAll;
    public float Weight { get; private set; }
    
    public SolverPriority(string name, 
        Func<Data, IEnumerable<TBuild>> getAll,
        Func<Data, Regime, float> getWeight) 
    {
        Name = name;
        _getWeight = getWeight;
        _getAll = getAll;
    }
    public void SetWeight(Data data, Regime regime)
    {
        Weight = _getWeight(data, regime);
    }

    public Dictionary<IModel, float> GetWishlistCosts(
        Regime regime, 
        Data d)
    {
        var expandedPool = BudgetPool.ConstructForRegime(regime, d);
        foreach (var i in expandedPool.AvailModels.Contents.Keys.ToList())
        {
            expandedPool.AvailModels.Contents[i] *= 2f;
        }
        SetCalcData(regime, d);
        var solver = MakeSolver();
        var projVars = MakeProjVars(solver, d);
        SetConstraints(solver, regime, expandedPool, projVars, d);
        var success = Solve(solver, projVars);
        if (success == false)
        {
            GD.Print("failed");
        }
        var toBuild = projVars
            .Where(v => v.Value.SolutionValue() > 0f)
            .ToDictionary(v => v.Key, v => (int)v.Value.SolutionValue());
        return GetCosts(toBuild, d);
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
        if (success == false)
        {
            GD.Print("failed");
        }
        var toBuild = projVars
            .Where(v => v.Value.SolutionValue() > 0f)
            .ToDictionary(v => v.Key, v => (int)v.Value.SolutionValue());
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
    
    private bool Solve(Solver solver, 
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
        var status = solver.Solve();
        return status == Solver.ResultStatus.OPTIMAL || status == Solver.ResultStatus.FEASIBLE;
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
            var make = MakeProject.Construct(model, value);
            var proc = new StartMakeProjectProc(r.MakeRef(), make);
            key.SendMessage(proc);
        }
    }
}