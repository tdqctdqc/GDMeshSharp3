using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;


public abstract class SolverPriority<TBuild> : IBudgetPriority
    where TBuild : IIdentifiable, IMakeable
{
    public string Name { get; private set; }
    public Dictionary<Item, int> Wishlist { get; private set; }
    private Func<Data, Regime, float> _getWeight;
    private Func<Data, IEnumerable<TBuild>> _getAll;
    public float Weight { get; private set; }
    public BudgetAccount Account { get; private set; }
    
    
    private Func<TBuild, bool> _relevant;
    private Func<TBuild, float> _utility;
    public SolverPriority(string name, 
        Func<Data, IEnumerable<TBuild>> getAll,
        Func<Data, Regime, float> getWeight,
        Func<TBuild, bool> relevant,
        Func<TBuild, float> utility) 
    {
        Name = name;
        _getWeight = getWeight;
        _getAll = getAll;
        Account = new BudgetAccount();
        Wishlist = new Dictionary<Item, int>();
        _relevant = relevant;
        _utility = utility;
    }
    public void SetWeight(Data data, Regime regime)
    {
        Weight = _getWeight(data, regime);
    }
    public void Calculate(Regime regime, Data data, MajorTurnOrders orders)
    {
        var solver = MakeSolver();
        var projVars = MakeProjVars(solver, data);
        SetConstraints(solver, regime, projVars, data);
        
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

    protected abstract void SetConstraints(Solver solver, Regime r,
        Dictionary<TBuild, Variable> projVars, 
        Data data);
    protected abstract void SetWishlistConstraints(Solver solver, Regime r,
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
        SetWishlistConstraints(solver, regime, projVars, data, pool, proportion);
        
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
            kvp => kvp.Key.Makeable.ItemCosts.GetEnumerableModel(data).ToDictionary(kvp => kvp.Key, kvp => (int)kvp.Value), 
            (kvp, i) => Mathf.CeilToInt(i * kvp.Value.SolutionValue()));
    }
    
    protected Dictionary<TBuild, Variable> MakeProjVars(Solver solver, Data data)
    {
        var models = _getAll(data).Where(_relevant);
        return models.Select(b =>
        {
            var projVar = solver.MakeIntVar(0, 1000, b.Id.ToString());
            return new KeyValuePair<TBuild, Variable>(b, projVar);
        }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
    
    public void Wipe()
    {
        Account.Clear();
        Wishlist.Clear();
    }

    public void SetWishlist(Regime r, Data d, BudgetPool pool, float proportion)
    {
        Wishlist = CalculateWishlist(r, d, pool, proportion);
    }
    public void FirstRound(MajorTurnOrders orders, Regime regime, float proportion, 
        BudgetPool pool, Data data)
    {
        var taken = new BudgetAccount();
        taken.TakeShare(proportion, pool, data);
        Account.Add(taken);
        Calculate(regime, data, orders);
    }

    public void SecondRound(MajorTurnOrders orders, Regime regime, float proportion, 
        BudgetPool pool, Data data, float multiplier)
    {
        proportion = Mathf.Min(1f, multiplier * proportion);
        FirstRound(orders, regime, proportion, pool, data);
        ReturnUnused(pool, data);
    }

    private void ReturnUnused(BudgetPool pool, Data data)
    {
        foreach (var kvp in Account.Items.Contents)
        {
            var item = data.Models.GetModel<Item>(kvp.Key);
            var q = kvp.Value;
            if (Account.UsedItem.Contains(item) == false 
                && Wishlist.ContainsKey(item) == false)
            {
                Account.Items.Remove(item, q);
                pool.AvailItems.Add(item, q);
            }
        }
        
        foreach (var kvp in Account.Models.Contents)
        {
            var model = data.Models.GetModel<IModel>(kvp.Key);
            var q = kvp.Value;
            if (Account.UsedModel.Contains(model) == false)
            {
                Account.Models.Remove(model, q);
                pool.AvailModels.Add(model, q);
            }
        }

        if (Account.UsedLabor == false)
        {
            var labor = Account.Labor;
            Account.UseLabor(labor);
            pool.AvailLabor += labor;
        }
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
}