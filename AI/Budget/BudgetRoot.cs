
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BudgetRoot : BudgetBranch
{
    private BudgetBranch _construct, _military;
    public Dictionary<IModel, float> Prices { get; private set; }
    
    public Dictionary<PriorityNode, (float spent, int tick)> LastSpending { get; private set; }
    
    public BudgetRoot(Data d) : base("Root")
    {
        Prices = new Dictionary<IModel, float>();
        LastSpending = new Dictionary<PriorityNode, (float spent, int tick)>();
        _construct = new ConstructBuildingsBudgetBranch(d);
        Children.Add(_construct);

        _military = new MilitaryBudgetBranch(d);
        Children.Add(_military);
    }

    public void Calculate(Regime r, LogicWriteKey key)
    {
        SetWeights(r, key.Data);
        Bid(r, key);
    }
    
    private void Bid(Regime r, LogicWriteKey key)
    {
        var leaves = GetLeaves().ToArray();
        var buildCostPool = BudgetPool.ConstructForRegime(r, key.Data);
        SetPrices(r, key.Data, leaves, buildCostPool);
        
        foreach (var priorityNode in leaves)
        {
            var weight = priorityNode.GetTreeWeight(key.Data);
            priorityNode.Credit.AddCreditToCurrent(weight);
        }
        
        var valid = leaves.ToHashSet();
        var iter = 0;
        while (valid.Count > 0 && iter < 10)
        {
            iter++;
            var most = valid
                .MaxBy(v => v.Credit.GetCredit());
            var stillValid = most.Priority.Calculate(buildCostPool, r, key,
                out var modelCosts);
            if (stillValid == false)
            {
                valid.Remove(most);
            }
            else
            {
                var price = modelCosts.Sum(
                    kvp => kvp.Value * getModelPrice(kvp.Key));
                most.Credit.AddSpendingToCurrent(price);
                addSpending(most, price);
            }
        }

        float getModelPrice(IModel m)
        {
            if (Prices.TryGetValue(m, out var price)) return price;
            return 0f;
        }

        void addSpending(PriorityNode priority, float spent)
        {
            var tick = key.Data.GetTick();
            if (LastSpending.TryGetValue(priority, out var v)
                && v.tick == tick)
            {
                LastSpending[priority] = (v.spent + spent, tick);
            }
            else
            {
                LastSpending[priority] = (spent, tick);
            }
        }
    }
    
    public void
        SetPrices(Regime r,
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