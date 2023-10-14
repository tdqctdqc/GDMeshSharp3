using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class TradeProcedure : Procedure
{
    public List<RegimeItemTradeReport> RegimeItemTradeReports { get; private set; }
    public Dictionary<int, ItemTradeReport> ItemTradeInfos { get; private set; }
    public Dictionary<int, float> RegimeTradeBalances { get; private set; }
    public Dictionary<int, float> NewPrices { get; private set; }
    
    public static TradeProcedure Construct()
    {
        return new TradeProcedure(new List<RegimeItemTradeReport>(), new Dictionary<int, float>(),
            new Dictionary<int, float>(), new Dictionary<int, ItemTradeReport>());
    }
    [SerializationConstructor] private TradeProcedure(List<RegimeItemTradeReport> regimeItemTradeReports, 
        Dictionary<int, float> regimeTradeBalances,
        Dictionary<int, float> newPrices,
        Dictionary<int, ItemTradeReport> itemTradeInfos)
    {
        RegimeItemTradeReports = regimeItemTradeReports;
        RegimeTradeBalances = regimeTradeBalances;
        NewPrices = newPrices;
        ItemTradeInfos = itemTradeInfos;
    }

    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        foreach (var r in key.Data.GetAll<Regime>())
        {
            r.Finance.ClearTradeBalance(key);
        }
        var market = key.Data.Society.Market;
        var tick = key.Data.BaseDomain.GameClock.Tick;
        foreach (var tradeReport in RegimeItemTradeReports)
        {
            var regime = key.Data.Get<Regime>(tradeReport.RegimeId);
            var item = (Item) key.Data.Models[tradeReport.ItemId];
            var q = tradeReport.Net();
            if (q > 0)
            {
                regime.Items.Add(item, q);
            }
            else
            {
                regime.Items.Remove(item, -q);
            }

            var itemReport = regime.History.ItemHistory.Get(item, tick);
            itemReport.Bought = tradeReport.QuantityBought;
            itemReport.Sold = tradeReport.QuantitySold;
            itemReport.Offered = tradeReport.QuantityOffered;
            itemReport.Demanded = tradeReport.QuantityDemanded;
        }
        
        foreach (var kvp in RegimeTradeBalances)
        {
            var regime = key.Data.Get<Regime>(kvp.Key);
            var balance = kvp.Value;
            regime.Finance.AddToTradeBalance(balance, key);
        }
        
        foreach (var kvp in NewPrices)
        {
            var item = (Item) key.Data.Models[kvp.Key];
            market.SetNewPrice(item, kvp.Value, tick, key);
        }
        
        market.WriteHistory(ItemTradeInfos, tick, key);
    }
    
    
}
