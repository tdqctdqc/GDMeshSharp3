using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class TradeModule : LogicModule
{
    public static float MaxPriceDeviationRatioFromDefault { get; private set; } = 10f;
    public static float PriceAdjustmentRatio { get; private set; } = .25f;
    public override LogicResults Calculate(List<TurnOrders> orders, Data data)
    {
        var res = new LogicResults();
        var proc = TradeProcedure.Construct();
        res.Messages.Add(proc);

        var buyOrders = orders.SelectMany(o => ((MajorTurnOrders) o).TradeOrders.BuyOrders).ToList();
        var sellOrders = orders.SelectMany(o => ((MajorTurnOrders) o).TradeOrders.SellOrders).ToList();
        
        var tradeable = data.Models.GetModels<Item>().Values.Where(i => i is TradeableItem);
        var infos = tradeable.ToDictionary(i => i, i => ItemTradeReport.Construct(i, data));
        var market = data.Society.Market;

        SetupInfos(infos, sellOrders, buyOrders, data, proc);
        ExchangeItems(infos, sellOrders, buyOrders, data, proc);
        UpdatePrices(infos, proc, data);
        
        return res;
    }

    private void SetupInfos(Dictionary<Item, ItemTradeReport> infos, List<SellOrder> sellOrders, 
        List<BuyOrder> buyOrders, Data data, TradeProcedure proc)
    {
        var tick = data.Tick;
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
    private void UpdatePrices(Dictionary<Item, ItemTradeReport> infos, TradeProcedure proc, Data data)
    {
        var market = data.Society.Market;
        foreach (var kvp in infos)
        {
            var info = kvp.Value;
            var item = (TradeableItem)kvp.Key;
            var price = market.Prices[item.Id];
            var offered = info.TotalOffered;
            var demanded = info.TotalDemanded;
            if (offered > demanded)
            {
                var minPrice = item.DefaultPrice / MaxPriceDeviationRatioFromDefault;
                if (price == minPrice)
                {
                    // GD.Print($"Price of {item.Name} stuck at min price {minPrice}");
                }
                else
                {
                    var adjustmentRatio = 0f;
                    var diffRatio = (float)(offered - demanded) / (offered + demanded);
                    
                    if (demanded == 0) adjustmentRatio = PriceAdjustmentRatio;
                    else adjustmentRatio = Mathf.Min(1f, diffRatio) * PriceAdjustmentRatio;

                    var newPrice = price * (1f - adjustmentRatio);
                
                    if (newPrice > minPrice)
                    {
                        if (newPrice < price)
                        {
                            proc.NewPrices.Add(item.Id, newPrice);
                        }
                    }
                    else 
                    {
                        proc.NewPrices.Add(item.Id, minPrice);
                    }
                }
            }
            else if (demanded > offered)
            {
                var maxPrice = item.DefaultPrice * MaxPriceDeviationRatioFromDefault;
                if (price == maxPrice)
                {
                }
                else
                {
                    var adjustmentRatio = 0f;
                    var diffRatio = (float)(demanded - offered) / (offered + demanded);
                
                    if (offered == 0) adjustmentRatio = PriceAdjustmentRatio;
                    else adjustmentRatio = Mathf.Min(1f, diffRatio) * PriceAdjustmentRatio;

                    var newPrice = price * (1f + adjustmentRatio);
                
                    if (demanded == 0) adjustmentRatio = PriceAdjustmentRatio;
                    else adjustmentRatio = Mathf.Min(1f, diffRatio) * PriceAdjustmentRatio;
                
                    if (newPrice < maxPrice)
                    {
                        proc.NewPrices.Add(item.Id, newPrice);
                    }
                    else 
                    {
                        proc.NewPrices.Add(item.Id, maxPrice);
                    }
                }
            }
        }
        
    }
}
