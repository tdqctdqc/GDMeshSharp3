using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TradeModule : LogicModule
{
    public override LogicResults Calculate(Data data)
    {
        var res = new LogicResults();
        var proc = TradeProcedure.Construct();
        res.Procedures.Add(proc);
        
        //dummy
        var buyOrders = new List<BuyOrder>();
        var sellOrders = new List<SellOrder>();
        var infos = new Dictionary<Item, ItemTradeInfo>();
        var market = data.Society.Market;

        foreach (var sellOrder in sellOrders)
        {
            var item = (TradeableItem) data.Models[sellOrder.ItemId];
            if(infos.ContainsKey(item) == false) infos.Add(item, new ItemTradeInfo());
            infos[item].TotalOffered += sellOrder.Quantity;
        }
        foreach (var buyOrder in buyOrders)
        {
            var item = (TradeableItem) data.Models[buyOrder.ItemId];
            if(infos.ContainsKey(item) == false) infos.Add(item, new ItemTradeInfo());
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
        }
        
        foreach (var buyOrder in buyOrders)
        {
            var regime = data.Society.Regimes[buyOrder.RegimeId];
            var item = (TradeableItem) data.Models[buyOrder.ItemId];
            var info = infos[item];
            var q = Mathf.FloorToInt(buyOrder.Quantity * info.BuySatisfyRatio);
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

    private class ItemTradeInfo
    {
        public int TotalOffered { get; set; } = 0;
        public int TotalDemanded { get; set; } = 0;
        public float SellSatisfyRatio { get; set; } = 0;
        public float BuySatisfyRatio { get; set; } = 0;
    }
}
