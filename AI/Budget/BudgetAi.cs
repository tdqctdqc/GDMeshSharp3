using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BudgetAi 
{
    private Regime _regime;
    public List<BudgetPriority> Priorities { get; private set; }
    public IncomeBudget IncomeBudget { get; private set; }
    public BudgetItemReserve Reserve { get; private set; }
    
    public BudgetAi(Data data, Regime regime)
    {
        _regime = regime;
        Priorities = new List<BudgetPriority>
        {
               new FlowProdBuildingConstructionPriority(
                   data.Models.Flows.IndustrialPower, (r,d) => 1f),
               new FlowProdBuildingConstructionPriority(
                   data.Models.Flows.Income, (r,d) => .5f),
        };
        IncomeBudget = new IncomeBudget();
        Reserve = new BudgetItemReserve();
    }

    public void Calculate(Data data, 
        MajorTurnOrders orders)
    {
        IncomeBudget.Calculate(data);
        Reserve.Calculate(_regime, data);
        var prices = data.Society.Market.Prices
            .ToDictionary(kvp => (Item)data.Models[kvp.Key], 
                kvp => kvp.Value);
        
        foreach (var p in Priorities)
        {
            p.SetWeight(data, _regime);
        }
        var scratch = new BudgetScratch();
        
        DoMainRound(scratch, data, prices, orders);
        DoCleanupRound(scratch, data, prices, orders);
        HandleWishlistAndReserve(data, orders, scratch.Items);
    }

    private void DoMainRound(BudgetScratch scratch, Data data,
        Dictionary<Item, float> prices, MajorTurnOrders orders)
    {
        var totalPriorityWeight = Priorities.Sum(p => p.Weight);
        var totalLaborAvail = _regime.GetPolys(data).Sum(p => p.GetLaborSurplus(data));
        var totalPrice =
            _regime.Items.Contents.Sum(kvp =>
            {
                return prices.ContainsKey((Item) data.Models[kvp.Key])
                    ? prices[(Item) data.Models[kvp.Key]] * _regime.Items[kvp.Key]
                    : 1f;
            });
        foreach (var p in Priorities)
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
        foreach (var p in Priorities)
        {
            p.Calculate(_regime, data, scratch, prices, orders);
        }
    }
    public Dictionary<Item, int> GetItemWishlist(Data data, float credit)
    {
        var buyItemsIncome = Mathf.Floor(credit * IncomeBudget.BuyWishlistItemsRatio);
        var market = data.Society.Market;
        var prices = market.Prices.ToDictionary(kvp => (Item)data.Models[kvp.Key], kvp => kvp.Value);
        
        var totalLaborAvail = _regime.GetPolys(data).Sum(p => p.GetLaborSurplus(data));
        var totalPriorityWeight = Priorities.Sum(p => p.Weight);
        var creditBig = Mathf.Clamp(credit * 10, 0f, 1_000_000f);
        return Priorities.Select(p =>
            {
                var priorityWeight = p.Weight;
                var priorityShare = priorityWeight / totalPriorityWeight;
                var credit = buyItemsIncome * priorityShare;
                var noCreditLimit = p.GetTradeWishlist(_regime, data, prices,
                    Mathf.FloorToInt(creditBig), totalLaborAvail);
                var priceTotal = noCreditLimit.Sum(kvp => prices[kvp.Key] * kvp.Value);
                var ratioFulfil = Mathf.Clamp(credit / priceTotal, 0f, 1f);
                if (priceTotal == 0f) ratioFulfil = 1f;
                return noCreditLimit.ToDictionary(kvp => kvp.Key, kvp => Mathf.FloorToInt(kvp.Value * ratioFulfil));
            })
            .GetCounts(t => t);
    }

    private void HandleWishlistAndReserve(Data data, MajorTurnOrders orders, ItemCount itemBudget)
    {
        var income = _regime.Flows[data.Models.Flows.Income].Net();
        var buyWishlistItemsIncome = Mathf.Floor(income * IncomeBudget.BuyWishlistItemsRatio);
        var stockUpReserveItemsIncome = Mathf.Floor(income * IncomeBudget.BuyReserveItemsRatio);
        var wishlist = GetItemWishlist(data, buyWishlistItemsIncome);

        var reserveWishlist = new Dictionary<Item, int>();
        foreach (var kvp in itemBudget.Contents)
        {
            var item = (Item)data.Models[kvp.Key];
            if (item is TradeableItem t == false) continue;
            var q = Mathf.FloorToInt(kvp.Value);
            var reserve = Reserve.DesiredReserves.ContainsKey(item)
                ? Reserve.DesiredReserves[item]
                : 0;
            var needed = reserve - q;
            reserveWishlist.Add(item, needed);
        }
        Manufacture(data, wishlist, itemBudget, reserveWishlist, orders);
        
        DoTradeOrders(data, orders, itemBudget, buyWishlistItemsIncome, 
            stockUpReserveItemsIncome, wishlist, reserveWishlist);
    }

    private void Manufacture(Data data, Dictionary<Item, int> wishlist, ItemCount itemBudget,
        Dictionary<Item, int> reserveWishlist, MajorTurnOrders turnOrders)
    {
        var ip = data.Models.Flows.IndustrialPower;
        var ipUsed = _regime.ManufacturingQueue.Queue
            .Sum(m => m.Value().Remaining(data));
        var ipAvail = _regime.Flows[ip].Net() - ipUsed;
        if (ipAvail <= 0) return;

        var itemsWished = wishlist.ToList();
        for (var i = 0; i < itemsWished.Count; i++)
        {
            if (ipAvail <= 0) return;
            var kvp = itemsWished[i];
            manufacture(wishlist, kvp.Key, kvp.Value);
        }

        var reserveWished = reserveWishlist.ToList();
        for (var i = 0; i < reserveWished.Count; i++)
        {
            if (ipAvail <= 0) return;
            var kvp = reserveWished[i];
            manufacture(reserveWishlist, kvp.Key, kvp.Value);
        }

        void manufacture(Dictionary<Item, int> dic, Item item, int q)
        {
            if (item.Attributes.Has<ManufactureableAttribute>() == false) return;
            var manuf = item.Attributes.Get<ManufactureableAttribute>();
            var costPer = manuf.IndustrialCost;
            var itemCosts = manuf.ItemCosts;
            
            var minFulfilledItemRatio = itemCosts.Count > 0
                ? itemCosts.Min(kvp =>
                Mathf.Clamp(itemBudget[kvp.Key] / kvp.Value, 0f, 1f))
                : 1f;
            
            var possibleQ = Mathf.FloorToInt(Mathf.Min(q * minFulfilledItemRatio, ipAvail / costPer));
            if (possibleQ <= 0) return;
            var totalCost = costPer * possibleQ;
            ipAvail -= totalCost;
            dic[item] -= possibleQ;
            foreach (var kvp in itemCosts)
            {
                var inputItem = kvp.Key;
                var inputItemQ = kvp.Value * possibleQ;
                itemBudget.Remove(inputItem, inputItemQ);
            }
            var order = new ItemManufactureProject(-1, 0f, possibleQ, item.MakeRef());
            turnOrders.ManufacturingOrders.ToStart.Add(
                PolymorphMessage<ManufactureProject>.Construct(order, data));
            // GD.Print($"{_regime.Name} adding manuf project {possibleQ} {item.Name}");
        }
    }
    private void DoTradeOrders(Data data, MajorTurnOrders orders, ItemCount itemBudget,
        float buyItemsIncome,
        float stockUpReserveItemsIncome,
        Dictionary<Item, int> wishlist,
        Dictionary<Item, int> reserveWishlist)
    {
        var market = data.Society.Market;
        
        foreach (var kvp in wishlist)
        {
            orders.TradeOrders.BuyOrders.Add(new BuyOrder(kvp.Key.Id, _regime.Id, kvp.Value));
        }
        foreach (var kvp in reserveWishlist)
        {
            var item = kvp.Key;
            var needed = kvp.Value;
            if (needed == 0) continue;
            if (needed < 0)
            {
                orders.TradeOrders.SellOrders.Add(new SellOrder(item.Id, _regime.Id, -needed));
            }
            else if (needed > 0)
            {
                var price = market.Prices[item.Id];
                var qToBuy = Math.Min(needed, Mathf.FloorToInt(stockUpReserveItemsIncome / price));
                var spent = qToBuy * price;
                stockUpReserveItemsIncome -= spent;
                orders.TradeOrders.BuyOrders.Add(new BuyOrder(item.Id, _regime.Id, qToBuy));
            }
        }
    }
}
