
using System;
using System.Collections.Generic;
using System.Linq;

public class BudgetRoot : BudgetBranch
{
    private BudgetBranch _construct, _military;
    public BudgetRoot(Data d)
    {
        _construct = new ConstructBuildingsBudgetBranch(d);
        Children.Add(_construct);

        _military = new MilitaryBudgetBranch(d);
        Children.Add(_military);
    }

    public void Calculate(Regime r, LogicWriteKey key)
    {
        SetWeights(1f, r, key.Data);
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

        var buildCostPool = BudgetPool.ConstructForRegime(r, key.Data);
        
        var modelPrices = GetPrices(r, key.Data, leaves, buildCostPool);
        var valid = leaves.ToHashSet();
        var iter = 0;
        while (valid.Count > 0 && iter < 10)
        {
            iter++;
            var most = valid
                .MaxBy(v => v.Credit.GetCredit());
            var stillValid = most.Priority.Calculate(buildCostPool, r, key,
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

    public override void SetWeights(float selfWeight, Regime r, Data d)
    {
        _construct.SetWeights(.75f, r, d);
        _military.SetWeights(.25f, r, d);
    }
}