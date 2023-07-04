using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BudgetAi 
{
    private Regime _regime;
    private List<BudgetPriority> _priorities;
    public IncomeBudget IncomeBudget { get; private set; }
    public BudgetItemReserve Reserve { get; private set; }
    
    public BudgetAi(Data data, Regime regime)
    {
        _regime = regime;
        _priorities = new List<BudgetPriority>
        {
               new FlowProdBuildingConstructionPriority(FlowManager.IndustrialPower, (r,d) => 1f),
        };
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
        
        
        foreach (var p in _priorities)
        {
            p.SetWeight(data, _regime);
        }
        var totalPriorityWeight = _priorities.Sum(p => p.Weight);
        var budget = ItemCount.Construct(_regime.Items);
        foreach (var p in _priorities)
        {
            DoPriority(p, data, prices, budget, totalPriorityWeight, totalPrice, 
                totalLaborAvail, orders);
        }
        
    }

    private void DoPriority(BudgetPriority priority, Data data, Dictionary<Item, float> prices, 
        ItemCount itemBudget, float totalPriorityWeight, float totalPrice, int totalLaborAvail, 
        MajorTurnOrders orders)
    {
        var priorityWeight = priority.Weight;
        var priorityShare = priorityWeight / totalPriorityWeight;
        var credit = Mathf.FloorToInt(totalPrice *  priorityShare);
        var labor = Mathf.FloorToInt(priorityShare * totalLaborAvail);
        var constructCap = _regime.FlowCount[FlowManager.ConstructionCap];
        if (credit < 0f)
        {
            throw new Exception($"priority weight {priorityWeight} " +
                                $"total weight {totalPriorityWeight} " +
                                $"total price {totalPrice}");
        }
        priority.Calculate(_regime, data, itemBudget, prices, credit,
            labor, Mathf.FloorToInt(constructCap), orders);
        SpendIncome(data, orders, itemBudget);
    }

    public Dictionary<Item, int> GetItemWishlist(Data data, float credit)
    {
        var buyItemsIncome = Mathf.Floor(credit * IncomeBudget.BuyWishlistItemsRatio);
        var market = data.Society.Market;
        var prices = market.ItemPricesById.ToDictionary(kvp => (Item)data.Models[kvp.Key], kvp => kvp.Value);
        
        var totalLaborAvail = _regime.Polygons.Sum(p => p.GetLaborSurplus(data));
        var totalPriorityWeight = _priorities.Sum(p => p.Weight);
        
        return _priorities.Select(p =>
            {
                var priorityWeight = p.Weight;
                var priorityShare = priorityWeight / totalPriorityWeight;

                return p.GetItemWishlist(_regime, data, prices,
                    Mathf.FloorToInt(priorityShare * buyItemsIncome), totalLaborAvail);
            })
            .GetCounts(t => t);
    }

    private void SpendIncome(Data data, MajorTurnOrders orders, ItemCount itemBudget)
    {
        DoTradeOrders(data, orders, itemBudget);
    }
    private void DoTradeOrders(Data data, MajorTurnOrders orders, ItemCount itemBudget)
    {
        var market = data.Society.Market;
        var income = _regime.FlowCount[FlowManager.Income];
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
            var deficit = Mathf.CeilToInt(desired - qOnHand);
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
            var q = Mathf.FloorToInt(kvp.Value);
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
