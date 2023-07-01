using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RegimeHistory
{
    public ItemHistory ProdHistory { get; protected set; }
    public ItemHistory ConsumptionHistory { get; protected set; }
    public ItemHistory DemandHistory { get; protected set; }
    public PeepHistory PeepHistory { get; private set; }

    public static RegimeHistory Construct(Data data)
    {
        return new RegimeHistory(ItemHistory.Construct(data), ItemHistory.Construct(data),
            ItemHistory.Construct(data), PeepHistory.Construct());
    }

    [SerializationConstructor] private RegimeHistory(ItemHistory prodHistory,
        ItemHistory consumptionHistory, ItemHistory demandHistory, PeepHistory peepHistory)
    {
        ProdHistory = prodHistory;
        ConsumptionHistory = consumptionHistory;
        DemandHistory = demandHistory;
        PeepHistory = peepHistory;
    }
}
