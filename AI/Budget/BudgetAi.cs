using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BudgetAi
{
    private Regime _regime;
    private BudgetRoot _root;
    public BudgetAi(RegimeMilitaryAi milAi, Data data, Regime regime)
    {
        _regime = regime;
        _root = new BudgetRoot(data);
    }

    public void Calculate(LogicWriteKey key, MajorTurnOrders orders)
    {
        _root.Calculate(_regime, key);
    }

    private void Manufacture(Data data, Dictionary<Item, int> wishlist, BudgetPool pool,
         LogicWriteKey key)
     {
         // var ip = data.Models.Flows.IndustrialPower;
         // var backlogRatio = 3f;
         // var ipUsed = _regime.MakeQueue.Queue
         //     .Sum(m => m.Remaining(data));
         // var ipAvail = _regime.Flows.Get(ip).Net() * backlogRatio - ipUsed;
         // if (ipAvail <= 0) return;
         //
         // var itemsWished = wishlist.ToList();
         //
         // for (var i = 0; i < itemsWished.Count; i++)
         // {
         //     if (ipAvail <= 0) return;
         //     var item = itemsWished[i].Key;
         //     var q = wishlist[item];
         //     if (q < 0) throw new Exception();
         //     manufacture(item, q);
         // }
         //
         // void manufacture(Item item, int q)
         // {
         //     if (item is IMakeable m == false) return;
         //     var industrial = data.Models.Flows.IndustrialPower;
         //     var costPer = m.Makeable.BuildCosts.Get(industrial);
         //     var itemCosts = m.Makeable.BuildCosts.GetEnumerableModel(data);
         //     
         //     var minFulfilledItemRatio = itemCosts.Count() > 0
         //         ? itemCosts.Min(
         //             kvp =>
         //                Mathf.Clamp(pool.AvailModels.Get(kvp.Key) / kvp.Value, 0f, 1f))
         //         : 1f;
         //
         //     var inputItemLimit = q * minFulfilledItemRatio;
         //     var ipLimit = ipAvail / costPer;
         //     var possibleQ = Mathf.FloorToInt(Mathf.Min(inputItemLimit, ipLimit));
         //     possibleQ = Mathf.Min(wishlist[item], possibleQ);
         //     if (possibleQ == 0) return;
         //     if (possibleQ < 0) throw new Exception();
         //     var totalCost = costPer * possibleQ;
         //     ipAvail -= totalCost;
         //     wishlist[item] -= possibleQ;
         //     foreach (var kvp in itemCosts)
         //     {
         //         var input = kvp.Key;
         //         var inputQ = kvp.Value * possibleQ;
         //         pool.AvailModels.Remove(input, inputQ);
         //     }
         //
         //     var order = new ItemMakeProject(-1, 0f, possibleQ, item.MakeRef());
         //     var proc = new StartMakeProjectProc(_regime.MakeRef(), order);
         //     key.SendMessage(proc);
         // }
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
 
         foreach (var kvp in pool.AvailModels.Contents)
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