using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ItemTradeReport
{
    public int Tick { get; private set; }
    public float TotalOffered { get; set; } = 0;
    public float TotalDemanded { get; set; } = 0;
    public float TotalTraded { get; set; } = 0;
    public float Price { get; private set; } = 0;
    public float SellSatisfyRatio { get; set; } = 0;
    public float BuySatisfyRatio { get; set; } = 0;

    public static ItemTradeReport Construct(Item item, Data data)
    {
        return new ItemTradeReport(data.Tick, data.Society.Market.Prices[item.Id], 0f, 0f, 0f,
            0f, 0f);
    }
    [SerializationConstructor] private ItemTradeReport(int tick, float price, float totalOffered, float totalDemanded, float totalTraded, 
        float sellSatisfyRatio, float buySatisfyRatio)
    {
        Tick = tick;
        Price = price;
        TotalOffered = totalOffered;
        TotalDemanded = totalDemanded;
        TotalTraded = totalTraded;
        SellSatisfyRatio = sellSatisfyRatio;
        BuySatisfyRatio = buySatisfyRatio;
    }
}
