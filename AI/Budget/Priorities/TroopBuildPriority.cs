using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.LinearSolver;


public class TroopBuildPriority : BuildPriority<Troop>
{
    
    public TroopBuildPriority(string name, 
        Func<Data, Regime, float> getWeight, Func<Troop, bool> relevant, 
        Func<Troop, float> utility) 
        : base(name, getWeight, relevant, utility)
    {
    }
    public override Dictionary<Item, int> GetWishlist(Regime regime, Data data, BudgetPool pool, float proportion)
    {
        throw new NotImplementedException();
    }

    protected override void SetConstraints(Solver solver, Dictionary<Troop, Variable> projVars, Data data)
    {
        throw new NotImplementedException();
    }

    protected override void SetWishlistConstraints(Solver solver, Dictionary<Troop, Variable> projVars, Data data, BudgetPool pool, float proportion)
    {
        throw new NotImplementedException();
    }

    protected override void Complete(Regime r, MajorTurnOrders orders, Dictionary<Troop, int> toBuild, Data data)
    {
        throw new NotImplementedException();
    }

    
}