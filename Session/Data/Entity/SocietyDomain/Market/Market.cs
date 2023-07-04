using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using Godot;

public class Market : Entity
{
    public Dictionary<int, float> ItemPricesById { get; private set; }
    public ItemHistory OfferedHistory { get; private set; }
    public ItemHistory DemandedHistory { get; private set; }
    public ItemHistory TradedQHistory { get; private set; }
    public ItemHistory PriceHistory { get; private set; }
    public static Market Create(CreateWriteKey key)
    {
        var prices = key.Data.Models.Items.Models.Values
            .SelectWhereOfType<Item, TradeableItem>()
            .ToDictionary(item => item.Id, item => item.DefaultPrice);
        var m = new Market(key.IdDispenser.GetID(), prices,
            ItemHistory.Construct(), ItemHistory.Construct(), 
            ItemHistory.Construct(), ItemHistory.Construct());
        key.Create(m);
        return m;
    }
    [SerializationConstructor] private Market(int id, Dictionary<int, float> itemPricesById,
        ItemHistory offeredHistory,
        ItemHistory demandedHistory,
        ItemHistory tradedQHistory,
        ItemHistory priceHistory) 
        : base(id)
    {
        ItemPricesById = itemPricesById;
        OfferedHistory = offeredHistory;
        DemandedHistory = demandedHistory;
        TradedQHistory = tradedQHistory;
        PriceHistory = priceHistory;
    }

    public void SetNewPrice(Item item, float price, int tick, ProcedureWriteKey key)
    {
        var oldPrice = ItemPricesById[item.Id];
        //todo make float history class or genericize the key param in history?
        PriceHistory[item.Id].Add(Mathf.CeilToInt(oldPrice), tick);
        ItemPricesById[item.Id] = price;
    }

    public void WriteHistory(Dictionary<int, ItemTradeInfo> infos, int tick, ProcedureWriteKey key)
    {
        GD.Print("WRITING HISTORY");
        GD.Print(infos.Count);
        foreach (var kvp in infos)
        {
            var item = (Item) key.Data.Models[kvp.Key];

            DemandedHistory.Add(item, kvp.Value.TotalDemanded, tick);
            OfferedHistory.Add(item, kvp.Value.TotalOffered, tick);
            TradedQHistory.Add(item, kvp.Value.TotalTraded, tick);

            GD.Print("ITEM " + item.Name);
            GD.Print("\tOFFERED " + kvp.Value.TotalOffered);
            GD.Print("\tDEMANDED " + kvp.Value.TotalDemanded);
            GD.Print("\tTRADED " + kvp.Value.TotalTraded);
        }
    }
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(SocietyDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
}
