using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class TradeModule : LogicModule
{
    public static float MaxPriceDeviationRatioFromDefault { get; private set; } = 10f;
    public static float PriceAdjustmentRatio { get; private set; } = .25f;
    public override void Calculate(List<RegimeTurnOrders> orders, Data data, Action<Message> sendMessage)
    {
        var proc = TradeProcedure.Construct();
        sendMessage(proc);

        var buyOrders = orders.SelectMany(o => ((MajorTurnOrders) o).TradeOrders.BuyOrders).ToList();
        var sellOrders = orders.SelectMany(o => ((MajorTurnOrders) o).TradeOrders.SellOrders).ToList();
        
        var tradeable = data.Models.GetModels<Item>().Values.Where(i => i is TradeableItem);
        var infos = tradeable.ToDictionary(i => i, i => ItemTradeReport.Construct(i, data));
        var market = data.Society.Market;

        SetupInfosAvoidOverlap(infos, sellOrders, buyOrders, data, proc);
        ExchangeItems(infos, sellOrders, buyOrders, data, proc);
        UpdatePricesNew(infos, proc, data);
        
    }
private void SetupInfosAvoidOverlap(Dictionary<Item, ItemTradeReport> infos, List<SellOrder> sellOrders, 
        List<BuyOrder> buyOrders, Data data, TradeProcedure proc)
    {
        
        var sellByRegime = sellOrders
            .SortInto(o => new Vector2I(o.RegimeId, o.ItemId));
        var buyByRegime = buyOrders
            .SortInto(o => new Vector2I(o.RegimeId, o.ItemId));
        
        
        foreach (var kvp in sellByRegime)
        {
            if (buyByRegime.ContainsKey(kvp.Key) == false) continue;
            var sells = kvp.Value;
            var buys = buyByRegime[kvp.Key];
            while (sells.FirstOrDefault(o => o.Quantity > 0) is SellOrder so
                   && buys.FirstOrDefault(o => o.Quantity > 0) is BuyOrder bo)
            {
                var diff = Mathf.Abs(so.Quantity - bo.Quantity);
                if (so.Quantity > bo.Quantity)
                {
                    so.Quantity = diff;
                    bo.Quantity = 0;
                }
                else
                {
                    bo.Quantity = diff;
                    so.Quantity = 0;
                }
            }
            
        }
        
        foreach (var sellOrder in sellOrders)
        {
            var item = (TradeableItem) data.Models[sellOrder.ItemId];
            var price = data.Society.Market.Prices[item.Id];
            infos[item].TotalOffered += sellOrder.Quantity;
        }
        foreach (var buyOrder in buyOrders)
        {
            var item = (TradeableItem) data.Models[buyOrder.ItemId];
            var price = data.Society.Market.Prices[item.Id];
            infos[item].TotalDemanded += buyOrder.Quantity;
        }
            
        foreach (var kvp2 in infos)
        {
            var info = kvp2.Value;
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
            proc.ItemTradeInfos.Add(kvp2.Key.Id, info);
        }
    }
    
    private void ExchangeItems(Dictionary<Item, ItemTradeReport> tradeReports, List<SellOrder> sellOrders, 
        List<BuyOrder> buyOrders, Data data, TradeProcedure proc)
    {
        var market = data.Society.Market;
        var rItemTradeReports = new Dictionary<Vector2, RegimeItemTradeReport>();

        Vector2 getKey(Regime r, Item i)
        {
            return new Vector2(r.Id, i.Id);
        }

        RegimeItemTradeReport GetOrAdd(Regime r, Item i)
        {
            var key = getKey(r, i);
            return rItemTradeReports.GetOrAdd(key,
                k =>
                {
                    var report = RegimeItemTradeReport.Construct(i.Id, r.Id);
                    proc.RegimeItemTradeReports.Add(report);
                    return report;
                });
        }
        foreach (var buyOrder in buyOrders)
        {
            var regime = data.Get<Regime>(buyOrder.RegimeId);
            var item = (TradeableItem) data.Models[buyOrder.ItemId];
            var tradeReport = tradeReports[item];
            var q = Mathf.FloorToInt(buyOrder.Quantity * tradeReport.BuySatisfyRatio);
            if (q == 0) continue;
            tradeReport.TotalTraded += q;
            var p = market.Prices[item.Id];

            var regimeItemTradeReport = GetOrAdd(regime, item);
            regimeItemTradeReport.QuantityDemanded += buyOrder.Quantity;
            regimeItemTradeReport.QuantityBought += q;
            proc.RegimeTradeBalances.AddOrSum(regime.Id, -p * q);
        }
        foreach (var sellOrder in sellOrders)
        {
            var regime = data.Get<Regime>(sellOrder.RegimeId);
            var item = (TradeableItem) data.Models[sellOrder.ItemId];
            var tradeReport = tradeReports[item];
            var q = Mathf.FloorToInt(sellOrder.Quantity * tradeReport.SellSatisfyRatio);
            if (q == 0) continue;
            var p = market.Prices[item.Id];
            
            var regimeItemTradeReport = GetOrAdd(regime, item);
            regimeItemTradeReport.QuantityOffered += sellOrder.Quantity;
            regimeItemTradeReport.QuantitySold += q;
            proc.RegimeTradeBalances.AddOrSum(regime.Id, p * q);
        }
    }

    private void UpdatePricesNew(Dictionary<Item, ItemTradeReport> infos, 
        TradeProcedure proc, Data data)
    {
        var market = data.Society.Market;
        foreach (var kvp in infos)
        {
            var info = kvp.Value;
            var item = (TradeableItem)kvp.Key;
            var price = market.Prices[item.Id];
            var offered = info.TotalOffered;
            var demanded = info.TotalDemanded;
            if (offered == 0 && demanded == 0) continue;
            var surplusRatio = (offered - demanded) / (offered + demanded);
            if (offered == 0) surplusRatio = -1f;
            if (demanded == 0) surplusRatio = 1f;

            surplusRatio = Mathf.Clamp(surplusRatio, -1f, 1f);
            var targetPrice = item.DefaultPrice;


            if (offered < demanded)
            {
                targetPrice = item.DefaultPrice 
                    + item.DefaultPrice * -surplusRatio * MaxPriceDeviationRatioFromDefault;
            }
            else if (offered > demanded)
            {
                targetPrice = item.DefaultPrice 
                              / (surplusRatio * MaxPriceDeviationRatioFromDefault);
            }
            
            var adjustment = (targetPrice - price) * PriceAdjustmentRatio;
            var newPrice = price + adjustment;
            newPrice = Mathf.Clamp(newPrice, 
                item.DefaultPrice / MaxPriceDeviationRatioFromDefault,
                item.DefaultPrice * MaxPriceDeviationRatioFromDefault);
            proc.NewPrices.Add(item.Id, newPrice);
        }
    }
}
