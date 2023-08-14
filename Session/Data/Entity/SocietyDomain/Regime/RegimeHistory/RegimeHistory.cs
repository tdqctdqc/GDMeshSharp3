using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using ItemHistory = History<RegimeItemReport, Item>;
using PeepHistory = History<RegimePeepsReport>;

public class RegimeHistory
{
    public ItemHistory ItemHistory { get; protected set; }
    public PeepHistory PeepHistory { get; private set; }

    public static RegimeHistory Construct(Data data)
    {
        var itemHistories = ItemHistory.Construct();
        foreach (var item in data.Models.GetModels<Item>().Values)
        {
            itemHistories.Add(item, -1, RegimeItemReport.Construct());
        }
        return new RegimeHistory(itemHistories, PeepHistory.Construct());
    }

    [SerializationConstructor] private RegimeHistory(ItemHistory itemHistory, 
        PeepHistory peepHistory)
    {
        ItemHistory = itemHistory;
        PeepHistory = peepHistory;
    }

    public void PrepareNewMajorTick(int tick, ProcedureWriteKey key)
    {
        foreach (var item in key.Data.Models.GetModels<Item>().Values)
        {
            ItemHistory.Add(item, tick, RegimeItemReport.Construct());
        }
    }
}
