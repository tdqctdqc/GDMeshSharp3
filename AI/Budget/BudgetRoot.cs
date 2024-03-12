
using System;
using System.Collections.Generic;
using System.Linq;

public class BudgetRoot : BudgetBranch
{
    
    public void Calculate(Regime r, LogicWriteKey key)
    {
        Bid(r, key);
    }

    private void Bid(Regime r, LogicWriteKey key)
    {
        var leaves = GetLeaves().ToArray();
        
        foreach (var priorityNode in leaves)
        {
            var weight = priorityNode.GetTreeWeight(key.Data);
            priorityNode.Credit.AddCreditToCurrent(weight);
        }

        var pool = BudgetPool.ConstructForRegime(r, key.Data);
        var modelPrices = GetPrices(r, key.Data, leaves, pool);
        var valid = leaves.ToHashSet();
        while (valid.Count > 0)
        {
            var most = valid
                .MaxBy(v => v.Credit.GetCredit());
            var stillValid = most.Priority.Calculate(pool, r, key,
                out var modelCosts);
            if (stillValid == false) valid.Remove(most);
            var price = modelCosts.Sum(
                kvp => kvp.Value * getModelPrice(kvp.Key));
            most.Credit.AddSpendingToCurrent(price);
        }

        float getModelPrice(IModel m)
        {
            if (modelPrices.TryGetValue(m, out var price)) return price;
            return 0f;
        }
    }

    private Dictionary<IModel, float>
        GetPrices(Regime r,
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
            var avail = pool.AvailModels.Get(model);
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

            var test = totalModelDemand.GetEnumerableModel(d)
                           .Sum(kvp => kvp.Value * modelPrices[kvp.Key]);
            if (test != 1f) throw new Exception("Total price is " + test);
        }

        return modelPrices;
    }
    public override float GetWeight()
    {
        return 1f;
    }
}