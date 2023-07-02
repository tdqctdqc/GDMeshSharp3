using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ItemTradeInfo
{
    public int TotalOffered { get; set; } = 0;
    public int TotalDemanded { get; set; } = 0;
    public int TotalTraded { get; set; } = 0;
    public float SellSatisfyRatio { get; set; } = 0;
    public float BuySatisfyRatio { get; set; } = 0;

    [SerializationConstructor] public ItemTradeInfo(int totalOffered, int totalDemanded, int totalTraded, 
        float sellSatisfyRatio, float buySatisfyRatio)
    {
        TotalOffered = totalOffered;
        TotalDemanded = totalDemanded;
        TotalTraded = totalTraded;
        SellSatisfyRatio = sellSatisfyRatio;
        BuySatisfyRatio = buySatisfyRatio;
    }
}
