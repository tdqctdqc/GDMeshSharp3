using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;

public abstract class BudgetPriority
{
    public string Name { get; private set; }
    public Dictionary<Item, int> Wishlist { get; private set; }
    private Func<Data, Regime, float> _getWeight;
    private HashSet<Item> _usedItems;
    private HashSet<Flow> _usedFlows;
    private bool _usedLabor;
    public float Weight { get; private set; }
    public BudgetAccount Account { get; private set; }
    public BudgetPriority(string name, Func<Data, Regime, float> getWeight)
    {
        Name = name;
        _getWeight = getWeight;
        _usedItems = new HashSet<Item>();
        _usedFlows = new HashSet<Flow>();
        _usedLabor = false;
        Account = new BudgetAccount();
        Wishlist = new Dictionary<Item, int>();
    }

    public void SetWeight(Data data, Regime regime)
    {
        Weight = _getWeight(data, regime);
    }

    public abstract void Calculate(Regime regime, Data data,
        MajorTurnOrders orders, HashSet<Item> usedItem,
        HashSet<Flow> usedFlow,
        ref bool usedLabor);

    public abstract Dictionary<Item, int> GetTradeWishlist(Regime regime, Data data,
        Dictionary<Item, float> prices,
        float credit,
        int availLabor);

    public void Wipe()
    {
        Account.UseLabor(Account.Labor);
        _usedItems.Clear();
        _usedFlows.Clear();
        _usedLabor = false;
        Wishlist.Clear();
    }

    public void SetWishlist(Regime r, Data d,
        Dictionary<Item, float> prices, float credit, float availLabor)
    {
        Wishlist = GetTradeWishlist(r, d, prices, credit, Mathf.FloorToInt(availLabor));
    }
    public void FirstRound(MajorTurnOrders orders, Regime regime, float proportion, 
        BudgetPool pool, Data data)
    {
        var taken = new BudgetAccount();
        taken.TakeShare(proportion, pool, data);
        Account.Add(taken);
        Calculate(regime, data, orders, _usedItems,
            _usedFlows, ref _usedLabor);
        ReturnUnused(taken, pool, data);
    }

    public void SecondRound(MajorTurnOrders orders, Regime regime, float proportion, 
        BudgetPool pool, Data data, float multiplier)
    {
        proportion = Mathf.Min(1f, multiplier * proportion);
        FirstRound(orders, regime, proportion, pool, data);
    }

    private void ReturnUnused(BudgetAccount taken, BudgetPool pool, Data data)
    {
        foreach (var kvp in taken.Items.Contents)
        {
            var item = data.Models.GetModel<Item>(kvp.Key);
            var q = kvp.Value;
            if (_usedItems.Contains(item) == false)
            {
                Account.Items.Remove(item, q);
                pool.AvailItems.Add(item, q);
            }
        }
        
        foreach (var kvp in taken.Flows.Contents)
        {
            var flow = data.Models.GetModel<Flow>(kvp.Key);
            var q = kvp.Value;
            if (_usedFlows.Contains(flow) == false)
            {
                Account.Flows.Remove(flow, q);
                pool.AvailFlows.Add(flow, q);
            }
        }

        if (_usedLabor == false)
        {
            var labor = Account.Labor;
            Account.UseLabor(labor);
            pool.AvailLabor += labor;
        }
    }
}
