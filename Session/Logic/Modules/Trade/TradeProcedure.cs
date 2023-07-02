using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class TradeProcedure : Procedure
{
    public List<ItemChange> ItemChanges { get; private set; }
    public Dictionary<int, ItemTradeInfo> ItemTradeInfos { get; private set; }
    public Dictionary<int, float> RegimeTradeBalances { get; private set; }
    public Dictionary<int, float> NewPrices { get; private set; }
    
    public static TradeProcedure Construct()
    {
        return new TradeProcedure(new List<ItemChange>(), new Dictionary<int, float>(),
            new Dictionary<int, float>(), new Dictionary<int, ItemTradeInfo>());
    }
    [SerializationConstructor] private TradeProcedure(List<ItemChange> itemChanges, 
        Dictionary<int, float> regimeTradeBalances,
        Dictionary<int, float> newPrices,
        Dictionary<int, ItemTradeInfo> itemTradeInfos)
    {
        ItemChanges = itemChanges;
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
        var market = key.Data.Society.Market;
        var tick = key.Data.BaseDomain.GameClock.Tick;
        foreach (var itemChange in ItemChanges)
        {
            var regime = key.Data.Society.Regimes[itemChange.RegimeId];
            var item = (Item) key.Data.Models[itemChange.ItemId];
            if (itemChange.Quantity > 0)
            {
                regime.Items.Add(item, itemChange.Quantity);
            }
            else
            {
                regime.Items.Remove(item, -itemChange.Quantity);
            }
        }
        
        foreach (var kvp in RegimeTradeBalances)
        {
            var regime = key.Data.Society.Regimes[kvp.Key];
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
    
    public class ItemChange
    {
        public int ItemId { get; set; }
        public int RegimeId { get; set; }
        public int Quantity { get; set; }

        public ItemChange(int itemId, int regimeId, int quantity)
        {
            ItemId = itemId;
            RegimeId = regimeId;
            Quantity = quantity;
        }
    }
}
