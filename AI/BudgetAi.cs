using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BudgetAi 
{
    private Regime _regime;
    private Dictionary<Item, BudgetPriority> _priorities;
    public IncomeBudget IncomeBudget { get; private set; }
    public BudgetAi(Data data, Regime regime)
    {
        _regime = regime;
        _priorities = new Dictionary<Item, BudgetPriority>
        {
            {ItemManager.IndustrialPoint, 
                new ProdBuildingConstructBudgetPriority(ItemManager.IndustrialPoint, (r,d) => 1f)},
        };
        IncomeBudget = new IncomeBudget();
    }

    public void Calculate(Data data, 
        MajorTurnOrders orders)
    {
        var prices = data.Models.Items.Models.Values.ToDictionary(v => v, v => 1f);
        var totalPrice =
            _regime.Items.Contents.Sum(kvp => prices[(Item) data.Models[kvp.Key]] * _regime.Items[kvp.Key]);
        var totalLaborAvail = _regime.Polygons.Sum(p => p.GetLaborSurplus(data));
        
        
        foreach (var kvp in _priorities)
        {
            kvp.Value.SetWeight(data, _regime);
        }
        var totalPriorityWeight = _priorities.Sum(kvp => kvp.Value.Weight);
        var budget = ItemWallet.Construct(_regime.Items);
        foreach (var kvp in _priorities)
        {
            DoPriority(kvp.Value, data, prices, budget, totalPriorityWeight, totalPrice, 
                totalLaborAvail, orders);
        }
        
    }

    private void DoPriority(BudgetPriority priority, Data data, Dictionary<Item, float> prices, 
        ItemWallet budget, float totalPriorityWeight, float totalPrice, int totalLaborAvail, 
        MajorTurnOrders orders)
    {
        var priorityWeight = priority.Weight;
        var priorityShare = priorityWeight / totalPriorityWeight;
        var credit = Mathf.FloorToInt(totalPrice *  priorityShare);
        var labor = Mathf.FloorToInt(priorityShare * totalLaborAvail);
        if (credit < 0f)
        {
            throw new Exception($"priority weight {priorityWeight} " +
                                $"total weight {totalPriorityWeight} " +
                                $"total price {totalPrice}");
        }
        priority.Calculate(_regime, data, budget, prices, credit,
            labor, orders);
        SpendIncome(data, orders);
    }

    public Dictionary<Item, int> GetItemWishlist(Data data)
    {
        var income = Flow.Income.GetFlow(_regime, data);
        var buyItemsIncome = Mathf.Floor(income * IncomeBudget.BuyItemsRatio);
        var market = data.Society.Market;
        var prices1 = market.ItemPricesById;
        var prices = prices1
            .ToDictionary(kvp => (Item)data.Models[kvp.Key], kvp => kvp.Value);
        
        var totalLaborAvail = _regime.Polygons.Sum(p => p.GetLaborSurplus(data));
        var totalPriorityWeight = _priorities.Sum(kvp => kvp.Value.Weight);
        
        return _priorities.Select(kvp =>
            {
                var priorityWeight = kvp.Value.Weight;
                var priorityShare = priorityWeight / totalPriorityWeight;
                return kvp.Value.GetItemWishlist(_regime, data, prices,
                    Mathf.FloorToInt(priorityShare * buyItemsIncome), totalLaborAvail);
            })
            .GetCounts(t => t);
    }

    private void SpendIncome(Data data, MajorTurnOrders orders)
    {
        DoTradeOrders(data, orders);
    }
    private void DoTradeOrders(Data data, MajorTurnOrders orders)
    {
        var income = Flow.Income.GetFlow(_regime, data);
        income += _regime.Finance.LastTradeBalance;
        if (income < 0) return;
        var buyItemsIncome = Mathf.Floor(income * IncomeBudget.BuyItemsRatio);
        var wishlist = GetItemWishlist(data);
        
        foreach (var kvp in wishlist)
        {
            orders.TradeOrders.BuyOrders.Add(new BuyOrder(kvp.Key.Id, _regime.Id, kvp.Value));
        }
        foreach (var kvp in _regime.Items.Contents)
        {
            
        }
    }
}
