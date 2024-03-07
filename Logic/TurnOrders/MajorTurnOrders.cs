using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class MajorTurnOrders : RegimeTurnOrders
{
    public TradeOrders TradeOrders { get; private set; }
    public static MajorTurnOrders Construct(int tick, Regime regime)
    {
        return new MajorTurnOrders(tick, regime.MakeRef(), 
            TradeOrders.Construct());
    }
    [SerializationConstructor] private MajorTurnOrders(int tick, ERef<Regime> regime,
        TradeOrders tradeOrders) 
        : base(tick, regime)
    {
        TradeOrders = tradeOrders;
    }
}
