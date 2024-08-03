
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BudgetRoot : BudgetBranch
{
    private Dictionary<IBudgetNode, BudgetBranch> _parents;
    public IdCount<IModel> Prices { get; private set; }

    public static BudgetRoot Construct(Regime r, Data d)
    {
        var b = new BudgetRoot(new List<IBudgetNode>(),
            1f, "Budget Root",
            IdCount<IModel>.Construct());

        var construct = ConstructBuildingsBudgetBranch.Construct(r, b, d);
        b.SetParent(construct, b);

        var military = MilitaryBudgetBranch.Construct(r, b, d);
        b.SetParent(military, b);

        var resources = ResourceExtractionBudgetBranch.Construct(r, b, d);
        b.SetParent(resources, b);
        
        return b;
    }

    public BudgetRoot(List<IBudgetNode> children, float weight, string name, IdCount<IModel> prices) : base(children, weight, name)
    {
        Prices = prices;
        _parents = new Dictionary<IBudgetNode, BudgetBranch>();
        CalcParents(this);
    }

    private void CalcParents(BudgetBranch b)
    {
        foreach (var child in b.Children)
        {
            _parents[child] = b;
            if (child is BudgetBranch b2)
            {
                CalcParents(b2);
            }
        }
    }

    public BudgetBranch GetParent(IBudgetNode n)
    {
        return _parents.TryGetValue(n, out var p)
            ? p
            : null;
    }

    public void SetParent(IBudgetNode n, BudgetBranch b)
    {
        b.Children.Add(n);
        _parents[n] = b;
    }

    public void Calculate(Regime r, LogicKey key)
    {
        SetWeights(r, this, key.Data);
        Bid(r, key);
    }
    
    private void Bid(Regime r, LogicKey key)
    {
        var tick = key.Data.GetTick();
        var leaves = GetLeaves().ToArray();
        var buildCostPool = BudgetPool.ConstructForRegime(r, key.Data);
        SetPrices(r, key.Data, leaves, buildCostPool);
        
        foreach (var priorityNode in leaves)
        {
            var weight = priorityNode.GetTreeWeight(this, key.Data);
            priorityNode.Credit.AddCreditToCurrent(weight);
        }
        
        foreach (var leaf in leaves.OrderByDescending(l => l.Credit.GetCredit()))
        {
            leaf.Priority.Calculate(buildCostPool, r, key,
                out var modelCosts,
                out var built);
            
            foreach (var (model, value) in modelCosts)
            {
                buildCostPool.Stock.Remove(model, Mathf.Min(value, buildCostPool.Stock.Get(model)));
            }
            
            
            if (built.Count() == 0) continue;
            var price = modelCosts.Sum(
                kvp => kvp.Value * Prices.Get(kvp.Key));
            leaf.Credit.AddSpendingToCurrent(price);
            if (leaf.MadeByTick.ContainsKey(tick) == false)
            {
                leaf.MadeByTick.Add(tick, new Dictionary<string, float>());
            }
            foreach (var (model, amt) in built)
            {
                leaf.MadeByTick[tick].AddOrSum(model, amt);
            }
        }
    }
    
    public void SetPrices(Regime r,
        Data d,
        PriorityNode[] nodes,
        BudgetPool pool)
    {
        var wishlists = nodes
            .Select(n => n.Priority.GetWishlistCosts(r, d));
        var totalModelDemand = IdCount<IModel>.Construct();
        foreach (var modelCosts in wishlists)
        {
            totalModelDemand.Add(modelCosts);
        }

        var modelPrices = new Dictionary<IModel, float>();
        var totalPrice = 0f;
        
        foreach (var (id, num) in totalModelDemand.Contents)
        {
            var model = d.Models.GetModel<IModel>(id);
            var avail = pool.Stock.Get(model);
            var price = num / avail;
            if (avail == 0f) price = 0f;
            modelPrices.Add(model, price);
            totalPrice += price * num;
        }

        if (totalPrice != 0f)
        {
            foreach (var model in modelPrices.Keys.ToList())
            {
                modelPrices[model] /= totalPrice;
            }

            var test = totalModelDemand.GetEnumModel(d)
                           .Sum(kvp => kvp.Value * modelPrices[kvp.Key]);
            
            if (Mathf.Abs(test - 1f) > .1f) throw new Exception("Total price is " + test);
        }

        
        Prices = IdCount<IModel>.Construct(modelPrices);
    }

    public Dictionary<IModel, float> RelativePrices(Data d)
    {
        if (Prices.Contents.Count > 0)
        {
            var min = Prices.Contents.Min(kvp => kvp.Value);
            if (min > 0f)
            {
                return Prices.Contents.ToDictionary(kvp => d.Models.GetModel<IModel>(kvp.Key),
                    kvp => kvp.Value / min);
            }
        }
        return new Dictionary<IModel, float>();
    }

    protected override float GetWeight(Regime r, BudgetRoot root, Data d)
    {
        return 1f;
    }
}