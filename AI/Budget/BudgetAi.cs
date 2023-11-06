using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BudgetAi
{
    private Regime _regime;
    public List<IBudgetPriority> Priorities { get; private set; }
    public IncomeBudget IncomeBudget { get; private set; }
    public BudgetAi(RegimeMilitaryAi milAi, Data data, Regime regime)
    {
        _regime = regime;
        Priorities = new List<IBudgetPriority>
        {
            new FlowProdBuildingConstructionPriority(
                data.Models.Flows.IndustrialPower, (d, r) => 1f),
            new FlowProdBuildingConstructionPriority(
                data.Models.Flows.Income, (d, r) => .25f),
            new ItemProdBuildingConstructionPriority(
                data.Models.Items.Recruits, (d, r) => .1f),
            new TroopBuildForTemplatePriority("Troops for templates", regime, 
                (d,r) => 1f,
                u => true, u => 1f),
            new FoodReservePriority(),
        };
    }

    public void Calculate(LogicWriteKey key, MajorTurnOrders orders)
    {
        foreach (var priority in Priorities)
        {
            priority.SetWeight(key.Data, _regime);
        }

        var itemsToDistribute = GetItemsToDistribute(key.Data);
        var labor = _regime.GetPolys(key.Data)
            .Sum(p => p.GetLaborSurplus(key.Data));
        var pool = new BudgetPool(itemsToDistribute, 
            IdCount<IModel>.Construct<IModel, Flow>(
                _regime.Flows.GetSurplusCount()), labor);
        DoPriorities(orders, pool, key);
    }

    private IdCount<Item> GetItemsToDistribute(Data data)
    {
        var itemsToDistribute = IdCount<Item>.Construct(_regime.Items);
        var itemsInAccounts = 
            IdCount<Item>.Union(Priorities.Select(v => v.Account.Items).ToArray());
        foreach (var kvp in itemsInAccounts.Contents)
        {
            var item = data.Models.GetModel<Item>(kvp.Key);
            var inAccountsQ = kvp.Value;
            var realQ = itemsToDistribute.Get(item);
            if (inAccountsQ > realQ)
            {
                var ratio = realQ / inAccountsQ;
                if (float.IsNaN(ratio)) ratio = 0f;
                var newQ = 0f;
                foreach (var priority in Priorities)
                {
                    if (priority.Account.Items.Contents.ContainsKey(item.Id) == false)
                    {
                        continue;
                    }
                    priority.Account.Items.Contents[item.Id] *= ratio;
                    newQ += priority.Account.Items.Contents[item.Id];
                }
        
                itemsInAccounts.Contents[item.Id] = newQ;
            }
        }
        itemsToDistribute.Subtract(itemsInAccounts);
        return itemsToDistribute;
    }
    
    private void DoPriorities(MajorTurnOrders orders, BudgetPool pool, LogicWriteKey key)
    {
        var totalPriority = Priorities.Sum(p => p.Weight);
        if (totalPriority <= 0f) throw new Exception();
        foreach (var priority in Priorities)
        {
            priority.Wipe();
            var proportion = priority.Weight / totalPriority;
            priority.SetWishlist(_regime, key.Data, pool, proportion);
            priority.FirstRound(orders, _regime, proportion, pool, key);
        }
        foreach (var priority in Priorities)
        {
            var proportion = priority.Weight / totalPriority;
            priority.SecondRound(orders, _regime, proportion, pool, key, 3f);
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
        Manufacture(key.Data, allWishlists, pool, key);
        DoTradeOrders(key.Data, orders, pool, allWishlists);
    }
    
    private void Manufacture(Data data, Dictionary<Item, int> wishlist, BudgetPool pool,
         LogicWriteKey key)
     {
         var ip = data.Models.Flows.IndustrialPower;
         var backlogRatio = 3f;
         var ipUsed = _regime.ManufacturingQueue.Queue
             .Sum(m => m.Remaining(data));
         var ipAvail = _regime.Flows.Get(ip).Net() * backlogRatio - ipUsed;
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
             if (item is IMakeable m == false) return;
             var costPer = m.Makeable.IndustrialCost;
             var itemCosts = m.Makeable.ItemCosts.GetEnumerableModel(data);
             
             var minFulfilledItemRatio = itemCosts.Count() > 0
                 ? itemCosts.Min(
                     kvp =>
                        Mathf.Clamp(pool.AvailItems.Get(kvp.Key) / kvp.Value, 0f, 1f))
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
             var proc = new StartManufacturingProjectProc(_regime.MakeRef(), order);
             key.SendMessage(proc);
         }
     }
    private void DoTradeOrders(Data data, MajorTurnOrders orders, BudgetPool pool, 
        Dictionary<Item, int> wishlist)
     {
         var market = data.Society.Market;
         var credits = pool.AvailModels.Get(data.Models.Flows.Income);

         var plausibleCosts = new Dictionary<Item, float>();
         foreach (var kvp in wishlist)
         {
             if (kvp.Key is TradeableItem == false) continue;
             var price = market.Prices[kvp.Key.Id];
             var latest = market.TradeHistory.GetLatest(kvp.Key);
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
             if (kvp.Key is TradeableItem == false) continue;
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
             orders.TradeOrders.SellOrders
                 .Add(new SellOrder(kvp.Key, _regime.Id, q));
         }
     }
}