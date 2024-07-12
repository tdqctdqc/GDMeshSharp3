
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class BuildTree
{
    public static float Increment(
        MakeProject proj, RegimeStock stock, StrongWriteKey key)
    {
        var makeable = ((IMakeable)proj.Making.Get(key.Data)).Makeable;
        var children = makeable.BuildCosts.GetEnumModel(key.Data)
                .Select(kvp => (kvp.Key, kvp.Value)).ToDictionary(
                    v => v.Item1, v => v.Item2);;
        var totalToMake = proj.Amount - proj.Fulfilled;
        if (totalToMake == 0f) return 0f;
        var made = Run(children, stock, totalToMake, key.Data);
        return made;
    }


   

    private static float Run(Dictionary<Item, float> children,
        RegimeStock stock, float totalToMake, Data d)
    {
        var made = 0f;
        var increment = 0f;
        do
        {
            increment = DoIter(children, stock, 
                totalToMake - made, d);
            made += increment;
        }
        while (increment > 0f && made < totalToMake);
        return made;
    }

    private static float DoIter(Dictionary<Item, float> children,
        RegimeStock stock, float leftToMake,
        Data d)
    {
        children = MakeChildren(children, stock, d);
        if (children is null) return 0f;
        
        var amount = leftToMake;
        foreach (var (model, unitCost) in children)
        {
            if (unitCost == 0f) continue;
            var avail = stock.Stock.Get(model);
            var canBuild = avail / unitCost;
            amount = Mathf.Min(amount, canBuild);
        }
        
        foreach (var (model, unitCost) in children)
        {
            if (unitCost == 0f) continue;
            var used = unitCost * amount;
            stock.Stock.Remove(model, used);
            stock.SingleTimeCosts.Add(model, used);
        }
        return amount;
    }

    private static Dictionary<Item, float> MakeChildren(
        Dictionary<Item, float> children,
        RegimeStock stock,
        Data d)
    {
        Dictionary<Item, float> newChildren = null;
        
        foreach (var (model, unitCost) in children)
        {
            var feasible = checkEntry(model, unitCost);
            if (feasible == false) return null;
        }

        if (newChildren is null) return children;
        return newChildren;

        bool checkEntry(Item m, float unitCost)
        {
            if (stock.Stock.Get(m) == 0f)
            {
                if (m is IMakeable makeable == false)
                {
                    return false;
                }
                
                if (newChildren is null)
                {
                    newChildren = new Dictionary<Item, float>();
                }
                
                foreach (var (m2, unitCost2) 
                         in makeable.Makeable.BuildCosts.GetEnumModel(d))
                {
                    var feasible = checkEntry(m2, unitCost2);
                    if (feasible == false) return false;
                }

                return true;
            }
            else
            {
                if (newChildren is not null)
                {
                    newChildren.AddOrSum(m, unitCost);
                }

                return true;
            }
        }
    }
}

