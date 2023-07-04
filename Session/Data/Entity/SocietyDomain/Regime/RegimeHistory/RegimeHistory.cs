using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RegimeHistory
{
    public ItemHistory ProdHistory { get; protected set; }
    public PeepHistory PeepHistory { get; private set; }

    public static RegimeHistory Construct(Data data)
    {
        return new RegimeHistory(ItemHistory.Construct(), PeepHistory.Construct());
    }

    [SerializationConstructor] private RegimeHistory(ItemHistory prodHistory, PeepHistory peepHistory)
    {
        ProdHistory = prodHistory;
        PeepHistory = peepHistory;
    }
}
