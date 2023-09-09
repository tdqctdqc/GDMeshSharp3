using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class NewBudgetAi
{
    private Regime _regime;
    public List<BudgetPriority> Priorities { get; private set; }
    
    public IncomeBudget IncomeBudget { get; private set; }
    public NewBudgetAi(Data data, Regime regime)
    {
        _regime = regime;
        Priorities = new List<BudgetPriority>
        {
            new FlowProdBuildingConstructionPriority(
                data.Models.Flows.IndustrialPower, (r, d) => 1f),
            new FlowProdBuildingConstructionPriority(
                data.Models.Flows.Income, (r, d) => .5f),
            new ItemProdBuildingConstructionPriority(
                data.Models.Items.Recruits, (r, d) => .1f),
            new FoodReservePriority()
        };
    }

    public void Calculate(Data data, MajorTurnOrders orders)
    {
        foreach (var priority in Priorities)
        {
            priority.SetWeight(data, _regime);
        }

        var itemsToDistribute = GetItemsToDistribute(data);
        var labor = _regime.GetPolys(data).Sum(p => p.GetLaborSurplus(data));
        var pool = new BudgetPool(itemsToDistribute, _regime.Flows.GetSurplusCount(), labor);
        DoPriorities(orders, pool, data);
    }

    private ItemCount GetItemsToDistribute(Data data)
    {
        var itemsToDistribute = ItemCount.Construct(_regime.Items);
        var itemsInAccounts = ItemCount.Union(Priorities
            .Select(v => v.Account.Items).ToArray());
        foreach (var kvp in itemsInAccounts.Contents)
        {
            var item = data.Models.GetModel<Item>(kvp.Key);
            var inAccountsQ = kvp.Value;
            var realQ = itemsToDistribute[item];
            if (inAccountsQ > realQ)
            {
                var ratio = realQ / inAccountsQ;
                if (float.IsNaN(ratio)) ratio = 0f;
                var newQ = 0f;
                foreach (var priority in Priorities)
                {
                    priority.Account.Items.Contents[item.Id] *= ratio;
                    newQ += priority.Account.Items.Contents[item.Id];
                }
        
                itemsInAccounts.Contents[item.Id] = newQ;
            }
        }
        itemsToDistribute.Subtract(itemsInAccounts);
        return itemsToDistribute;
    }
    
    private void DoPriorities(MajorTurnOrders orders, BudgetPool pool, Data data)
    {
        var market = data.Society.Market;
        var prices = market.Prices.ToDictionary(kvp => (Item)data.Models[kvp.Key], kvp => kvp.Value);
        var totalPriority = Priorities.Sum(p => p.Weight);
        if (totalPriority <= 0f) throw new Exception();
        foreach (var priority in Priorities)
        {
            priority.Wipe();
            var proportion = priority.Weight / totalPriority;
            priority.SetWishlist(_regime, data, pool.AvailLabor * proportion,
                pool.AvailFlows[data.Models.Flows.ConstructionCap] * proportion);
            priority.FirstRound(orders, _regime, proportion, pool, data);
        }
        foreach (var priority in Priorities)
        {
            var proportion = priority.Weight / totalPriority;
            priority.SecondRound(orders, _regime, proportion, pool, data, 3f);
        }
        var allWishlists = new Dictionary<Item, int>();
        foreach (var priority in Priorities)
        {
            var wishlist = priority.Wishlist;
            foreach (var kvp in wishlist)
            {
                if (kvp.Value < 0) throw new Exception();
                allWishlists.AddOrSum(kvp.Key, kvp.Value);
            }
        }
        Manufacture(data, allWishlists, pool, orders);
        DoTradeOrders(data, orders, pool, allWishlists);
    }
    
    private void Manufacture(Data data, Dictionary<Item, int> wishlist, BudgetPool pool,
         MajorTurnOrders turnOrders)
     {
         var ip = data.Models.Flows.IndustrialPower;
         var backlogRatio = 3f;
         var ipUsed = _regime.ManufacturingQueue.Queue
             .Sum(m => m.Value().Remaining(data));
         var ipAvail = _regime.Flows[ip].Net() * backlogRatio - ipUsed;
         if (ipAvail <= 0) return;

         var itemsWished = wishlist.ToList();
         
         for (var i = 0; i < itemsWished.Count; i++)
         {
             if (ipAvail <= 0) return;
             var item = itemsWished[i].Key;
             var q = wishlist[item];
             if (q < 0) throw new Exception();
             manufacture(item, q);
         }

         void manufacture(Item item, int q)
         {
             if (item.Attributes.Has<ManufactureableAttribute>() == false) return;
             var manuf = item.Attributes.Get<ManufactureableAttribute>();
             var costPer = manuf.IndustrialCost;
             var itemCosts = manuf.ItemCosts;
             
             var minFulfilledItemRatio = itemCosts.Count > 0
                 ? itemCosts.Min(
                     kvp =>
                        Mathf.Clamp(pool.AvailItems[kvp.Key] / kvp.Value, 0f, 1f))
                 : 1f;

             var inputItemLimit = q * minFulfilledItemRatio;
             var ipLimit = ipAvail / costPer;
             var possibleQ = Mathf.FloorToInt(Mathf.Min(inputItemLimit, ipLimit));
             possibleQ = Mathf.Min(wishlist[item], possibleQ);
             if (possibleQ == 0) return;
             if (possibleQ < 0) throw new Exception();
             var totalCost = costPer * possibleQ;
             ipAvail -= totalCost;
             wishlist[item] -= possibleQ;
             foreach (var kvp in itemCosts)
             {
                 var inputItem = kvp.Key;
                 var inputItemQ = kvp.Value * possibleQ;
                 pool.AvailItems.Remove(inputItem, inputItemQ);
             }

             var order = new ItemManufactureProject(-1, 0f, possibleQ, item.MakeRef());
             turnOrders.ManufacturingOrders.ToStart.Add(
                 PolymorphMessage<ManufactureProject>.Construct(order, data));
         }
     }
    private void DoTradeOrders(Data data, MajorTurnOrders orders, BudgetPool pool, 
        Dictionary<Item, int> wishlist)
     {
         var market = data.Society.Market;
         var credits = pool.AvailFlows[data.Models.Flows.Income];

         var plausibleCosts = new Dictionary<Item, float>();
         foreach (var kvp in wishlist)
         {
             var price = market.Prices[kvp.Key.Id];
             var latest = market.TradeHistory.Latest(kvp.Key);
             var plausibleCost = 0f;
             if (latest != null)
             {
                 plausibleCost = kvp.Value * latest.BuySatisfyRatio * price;
             }
             else plausibleCost = kvp.Value * price;

             plausibleCosts.Add(kvp.Key, plausibleCost);
         }
        
         var totalCost = plausibleCosts.Sum(kvp => kvp.Value);
         var buyRatio = Mathf.Clamp(credits / totalCost, 0f, 1f);
         
         if (float.IsNaN(buyRatio)) buyRatio = 0;
         
         foreach (var kvp in wishlist)
         {
             var buyQ = Mathf.FloorToInt(kvp.Value * buyRatio);
             if (buyQ < 0) throw new Exception();
             orders.TradeOrders.BuyOrders.Add(new BuyOrder(kvp.Key.Id, _regime.Id, 
                 buyQ));
         }
         
         foreach (var kvp in pool.AvailItems.Contents)
         {
             var item = data.Models.GetModel<Item>(kvp.Key);
             if (item is TradeableItem t == false) continue;
             var q = Mathf.FloorToInt(kvp.Value / 2);
             if (wishlist.ContainsKey(item))
             {
                 if (wishlist[item] >= q) continue;
                 q -= wishlist[item];
             }
             orders.TradeOrders.SellOrders.Add(new SellOrder(kvp.Key, _regime.Id,
                 q));
         }
     }
}