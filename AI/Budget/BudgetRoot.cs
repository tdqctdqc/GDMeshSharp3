
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BudgetRoot : BudgetBranch
{
    private BudgetBranch _construct, _military, _resources;
    public Dictionary<IModel, float> Prices { get; private set; }
    
    public BudgetRoot(Regime r, Data d) : base("Root")
    {
        Prices = new Dictionary<IModel, float>();
        _construct = new ConstructBuildingsBudgetBranch(r, this, d);
        Children.Add(_construct);

        _military = new MilitaryBudgetBranch(r, this, d);
        Children.Add(_military);
        _resources = new ResourceExtractionBudgetBranch(r, this, "Resource Extraction", d);
        Children.Add(_resources);
    }

    public void Calculate(Regime r, LogicKey key)
    {
        SetWeights(r, key.Data);
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
            var weight = priorityNode.GetTreeWeight(key.Data);
            priorityNode.Credit.AddCreditToCurrent(weight);
        }
        
        foreach (var leaf in leaves.OrderByDescending(l => l.Credit.GetCredit()))
        {
            var stillValid = leaf.Priority.Calculate(buildCostPool, r, key,
                out var modelCosts,
                out var built);
            if (built.Count() == 0) continue;
            var price = modelCosts.Sum(
                kvp => kvp.Value * getModelPrice(kvp.Key));
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

        float getModelPrice(IModel m)
        {
            if (Prices.TryGetValue(m, out var price)) return price;
            return 0f;
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

        
        Prices = modelPrices;
    }

    public Dictionary<IModel, float> RelativePrices()
    {
        if (Prices.Count > 0)
        {
            var min = Prices.Min(kvp => kvp.Value);
            if (min > 0f)
            {
                return Prices.ToDictionary(kvp => kvp.Key,
                    kvp => kvp.Value / min);
            }
        }
        return new Dictionary<IModel, float>();
    }

    protected override float GetWeight(Regime r, Data d)
    {
        return 1f;
    }
}