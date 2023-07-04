using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class TradeModule : LogicModule
{
    public override LogicResults Calculate(List<TurnOrders> orders, Data data)
    {
        var res = new LogicResults();
        var proc = TradeProcedure.Construct();
        res.Procedures.Add(proc);
        
        //dummy
        
        var buyOrders = new List<BuyOrder>();
        var sellOrders = new List<SellOrder>();
        foreach (var turnOrders in orders)
        {
            if (turnOrders is MajorTurnOrders m == false) throw new Exception();
            buyOrders.AddRange(m.TradeOrders.BuyOrders);
            sellOrders.AddRange(m.TradeOrders.SellOrders);
        }
        var infos = new Dictionary<Item, ItemTradeInfo>();
        var market = data.Society.Market;

        foreach (var sellOrder in sellOrders)
        {
            var item = (TradeableItem) data.Models[sellOrder.ItemId];
            if(infos.ContainsKey(item) == false) infos.Add(item, new ItemTradeInfo(0,0,0,0f,0f));
            infos[item].TotalOffered += sellOrder.Quantity;
        }
        foreach (var buyOrder in buyOrders)
        {
            var item = (TradeableItem) data.Models[buyOrder.ItemId];
            if(infos.ContainsKey(item) == false) infos.Add(item, new ItemTradeInfo(0,0,0,0f,0f));
            infos[item].TotalDemanded += buyOrder.Quantity;
        }
        
        foreach (var kvp in infos)
        {
            var info = kvp.Value;
            if (info.TotalOffered == 0 || info.TotalDemanded == 0)
            {
                info.SellSatisfyRatio = 0f;
                info.BuySatisfyRatio = 0f;
            }
            else
            {
                info.SellSatisfyRatio = Mathf.Clamp(info.TotalDemanded / info.TotalOffered, 0f, 1f);
                info.BuySatisfyRatio = Mathf.Clamp(info.TotalOffered / info.TotalDemanded, 0f, 1f);
            }
            proc.ItemTradeInfos.Add(kvp.Key.Id, info);
        }
        
        foreach (var buyOrder in buyOrders)
        {
            var regime = data.Society.Regimes[buyOrder.RegimeId];
            var item = (TradeableItem) data.Models[buyOrder.ItemId];
            var info = infos[item];
            var q = Mathf.FloorToInt(buyOrder.Quantity * info.BuySatisfyRatio);
            infos[item].TotalTraded += q;
            var p = market.ItemPricesById[item.Id];

            var itemChange = new TradeProcedure.ItemChange(item.Id, regime.Id, q);
            proc.ItemChanges.Add(itemChange);
            proc.RegimeTradeBalances.AddOrSum(regime.Id, -p * q);
        }
        foreach (var sellOrder in sellOrders)
        {
            var regime = data.Society.Regimes[sellOrder.RegimeId];
            var item = (TradeableItem) data.Models[sellOrder.ItemId];
            var info = infos[item];
            var q = Mathf.FloorToInt(sellOrder.Quantity * info.SellSatisfyRatio);
            var p = market.ItemPricesById[item.Id];
            
            var itemChange = new TradeProcedure.ItemChange(item.Id, regime.Id, -q);
            proc.ItemChanges.Add(itemChange);
            proc.RegimeTradeBalances.AddOrSum(regime.Id, p * q);
        }
        
        return res;
    }

    
}
