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
        var prices = data.Society.Market.ItemPricesById
            .ToDictionary(kvp => (Item)data.Models[kvp.Key], kvp => kvp.Value);
        
        
        foreach (var p in _priorities)
        {
            p.SetWeight(data, _regime);
        }
        var scratch = new BudgetScratch();
        
        DoMainRound(scratch, data, prices, orders);
        DoCleanupRound(scratch, data, prices, orders);
        SpendIncome(data, orders, scratch.Items);
    }

    private void DoMainRound(BudgetScratch scratch, Data data,
        Dictionary<Item, float> prices, MajorTurnOrders orders)
    {
        var totalPriorityWeight = _priorities.Sum(p => p.Weight);
        var totalLaborAvail = _regime.Polygons.Items(data).Sum(p => p.GetLaborSurplus(data));
        var totalPrice =
            _regime.Items.Contents.Sum(kvp =>
            {
                return prices.ContainsKey((Item) data.Models[kvp.Key])
                    ? prices[(Item) data.Models[kvp.Key]] * _regime.Items[kvp.Key]
                    : 1f;
            });
        foreach (var p in _priorities)
        {
            var weight = p.Weight;
            var share = weight / totalPriorityWeight;
            scratch.TakeShare(share, _regime.Items, _regime.Flows, totalLaborAvail, 
                Mathf.FloorToInt(totalPrice));
            
            p.Calculate(_regime, data, scratch, prices, orders);
        }
    }
    private void DoCleanupRound(BudgetScratch scratch, Data data,
        Dictionary<Item, float> prices, MajorTurnOrders orders)
    {
        scratch.MaxCredit();
        foreach (var p in _priorities)
        {
            p.Calculate(_regime, data, scratch, prices, orders);
        }
    }

    public Dictionary<Item, int> GetItemWishlist(Data data, float credit)
    {
        var buyItemsIncome = Mathf.Floor(credit * IncomeBudget.BuyWishlistItemsRatio);
        var market = data.Society.Market;
        var prices = market.ItemPricesById.ToDictionary(kvp => (Item)data.Models[kvp.Key], kvp => kvp.Value);
        
        var totalLaborAvail = _regime.Polygons.Items(data).Sum(p => p.GetLaborSurplus(data));
        var totalPriorityWeight = _priorities.Sum(p => p.Weight);
        var creditBig = Mathf.Clamp(credit * 10, 0f, 1_000_000f);
        return _priorities.Select(p =>
            {
                var priorityWeight = p.Weight;
                var priorityShare = priorityWeight / totalPriorityWeight;
                var credit = buyItemsIncome * priorityShare;
                var noCreditLimit = p.GetItemWishlist(_regime, data, prices,
                    Mathf.FloorToInt(creditBig), totalLaborAvail);
                var priceTotal = noCreditLimit.Sum(kvp => prices[kvp.Key] * kvp.Value);
                var ratioFulfil = Mathf.Clamp(credit / priceTotal, 0f, 1f);
                if (priceTotal == 0f) ratioFulfil = 1f;
                return noCreditLimit.ToDictionary(kvp => kvp.Key, kvp => Mathf.FloorToInt(kvp.Value * ratioFulfil));
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
        var income = _regime.Flows[FlowManager.Income].Net();
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
