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
                data.Models.Items.Recruits, (r, d) => .1f)
        };
    }

    public void Calculate(Data data, MajorTurnOrders orders)
    {
        var itemsToDistribute = ItemCount.Construct(_regime.Items);
        var itemsInAccounts = ItemCount.Union(Priorities
            .Select(v => v.Account.Items).ToArray());
        itemsToDistribute.Subtract(itemsInAccounts);
        var labor = _regime.GetPolys(data).Sum(p => p.GetLaborSurplus(data));
        var pool = new BudgetPool(itemsToDistribute, _regime.Flows.GetNetCount(), labor);
        
        DoPriorities(orders, pool, data);
        
    }

    private void DoPriorities(MajorTurnOrders orders, BudgetPool pool, Data data)
    {
        var market = data.Society.Market;
        var prices = market.Prices.ToDictionary(kvp => (Item)data.Models[kvp.Key], kvp => kvp.Value);
        var totalPriority = Priorities.Sum(p => p.Weight);
        if (totalPriority <= 0f) throw new Exception();
        var toSpend = pool.AvailFlows[data.Models.Flows.Income];
        foreach (var priority in Priorities)
        {
            priority.Wipe();
            var proportion = priority.Weight / totalPriority;
            priority.SetWishlist(_regime, data, prices, toSpend * proportion, pool.AvailLabor);
            priority.FirstRound(orders, _regime, proportion, pool, data);
        }
        foreach (var priority in Priorities)
        {
            var proportion = priority.Weight / totalPriority;
            priority.SecondRound(orders, _regime, proportion, pool, data, 3f);
        }
    }
    
    private void Manufacture(Data data, Dictionary<Item, int> wishlist, ItemCount itemBudget,
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
             var kvp = itemsWished[i];
             manufacture(wishlist, kvp.Key, kvp.Value);
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

             wishlist[item] -= possibleQ;
             var order = new ItemManufactureProject(-1, 0f, possibleQ, item.MakeRef());
             turnOrders.ManufacturingOrders.ToStart.Add(
                 PolymorphMessage<ManufactureProject>.Construct(order, data));
         }
     }
    private void DoTradeOrders(Data data, MajorTurnOrders orders, ItemCount itemBudget,
         float buyItemsIncome)
     {
         var market = data.Society.Market;
         
         
         
         // foreach (var kvp in wishlist)
         // {
         //     orders.TradeOrders.BuyOrders.Add(new BuyOrder(kvp.Key.Id, _regime.Id, kvp.Value));
         // }
     }
}