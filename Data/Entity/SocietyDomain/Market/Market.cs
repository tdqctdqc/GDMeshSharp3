using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using Godot;
using TradeHistory = History<ItemTradeReport, Item>;

public class Market : Entity
{
    public Dictionary<int, float> Prices { get; private set; }
    public TradeHistory TradeHistory { get; private set; }
    public static Market Create(ICreateWriteKey key)
    {
        var prices = key.Data.Models.GetModels<Item>().Values
            .SelectWhereOfType<TradeableItem>()
            .ToDictionary(item => item.Id, item => item.DefaultPrice);
        var m = new Market(key.Data.IdDispenser.TakeId(), prices,
            TradeHistory.Construct());
        key.Create(m);
        return m;
    }
    [SerializationConstructor] private Market(int id, Dictionary<int, float> prices,
        TradeHistory tradeHistory) 
        : base(id)
    {
        Prices = prices;
        TradeHistory = tradeHistory;
    }

    public void SetNewPrice(Item item, float price, int tick, ProcedureWriteKey key)
    {
        var oldPrice = Prices[item.Id];
        Prices[item.Id] = price;
    }

    public void WriteHistory(Dictionary<int, ItemTradeReport> infos, int tick, ProcedureWriteKey key)
    {
        foreach (var kvp in infos)
        {
            var item = key.Data.Models.GetModel<Item>(kvp.Key);
            TradeHistory.Add(item, tick, kvp.Value);
        }
    }
}
