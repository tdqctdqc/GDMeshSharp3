using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BudgetAi 
{
    private Regime _regime;
    private Dictionary<Item, BudgetPriority> _priorities;
    public IncomeBudget IncomeBudget { get; private set; }
    public BudgetItemReserve Reserve { get; private set; }
    
    public BudgetAi(Data data, Regime regime)
    {
        _regime = regime;
        // _priorities = new Dictionary<Item, BudgetPriority>
        // {
        //     {ItemManager.IndustrialPoint, 
        //         new ItemProdBuildingConstructionPriority(ItemManager.IndustrialPoint, (r,d) => 1f)},
        // };
        IncomeBudget = new IncomeBudget();
        Reserve = new BudgetItemReserve();
        
    }

    public void Calculate(Data data, 
        MajorTurnOrders orders)
    {
        IncomeBudget.Calculate(data);
        Reserve.Calculate(_regime, data);
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
        ItemWallet itemBudget, float totalPriorityWeight, float totalPrice, int totalLaborAvail, 
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
        priority.Calculate(_regime, data, itemBudget, prices, credit,
            labor, orders);
        SpendIncome(data, orders, itemBudget);
    }

    public Dictionary<Item, int> GetItemWishlist(Data data, float credit)
    {
        var buyItemsIncome = Mathf.Floor(credit * IncomeBudget.BuyWishlistItemsRatio);
        var market = data.Society.Market;
        var prices = market.ItemPricesById.ToDictionary(kvp => (Item)data.Models[kvp.Key], kvp => kvp.Value);
        
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

    private void SpendIncome(Data data, MajorTurnOrders orders, ItemWallet itemBudget)
    {
        DoTradeOrders(data, orders, itemBudget);
    }
    private void DoTradeOrders(Data data, MajorTurnOrders orders, ItemWallet itemBudget)
    {
        var market = data.Society.Market;
        var income = Flow.Income.GetNonBuildingFlow(_regime, data);
        income += _regime.Finance.LastTradeBalance;
        if (income < 0) return;
        
        var buyItemsIncome = Mathf.Floor(income * IncomeBudget.BuyWishlistItemsRatio);
        var wishlist = GetItemWishlist(data, buyItemsIncome);
        foreach (var kvp in wishlist)
        {
            orders.TradeOrders.BuyOrders.Add(new BuyOrder(kvp.Key.Id, _regime.Id, kvp.Value));
            var price = market.ItemPricesById[kvp.Key.Id];
        }
        
        var stockUpReserveItemsIncome = Mathf.Floor(income * IncomeBudget.BuyReserveItemsRatio);
        foreach (var kvp in Reserve.DesiredReserves)
        {
            if (stockUpReserveItemsIncome <= 0) break;
            var item = kvp.Key;
            var price = market.ItemPricesById[item.Id];
            var qOnHand = itemBudget[item];
            var desired = kvp.Value;
            var deficit = desired - qOnHand;
            if (deficit > 0)
            {
                var qToBuy = Math.Min(deficit, Mathf.FloorToInt(stockUpReserveItemsIncome / price));
                // GD.Print($"Deficit of {deficit} {item.Name}, buying {qToBuy}");

                var spent = qToBuy * price;
                stockUpReserveItemsIncome -= spent;
                orders.TradeOrders.BuyOrders.Add(new BuyOrder(item.Id, _regime.Id, qToBuy));
            }
        }
        
        foreach (var kvp in itemBudget.Contents)
        {
            var item = (Item)data.Models[kvp.Key];
            if (item is TradeableItem t == false) continue;
            var q = kvp.Value;
            var reserve = Reserve.DesiredReserves.ContainsKey(item)
                ? Reserve.DesiredReserves[item]
                : 0;
            var surplus = q - reserve;
            if (surplus > 0)
            {
                orders.TradeOrders.SellOrders.Add(new SellOrder(item.Id, _regime.Id, q));
            }
        }
    }
}
