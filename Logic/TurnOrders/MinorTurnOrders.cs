using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class MinorTurnOrders : RegimeTurnOrders
{
    public static MinorTurnOrders Construct(int tick, Regime regime)
    {
        return new MinorTurnOrders(tick, regime.MakeRef());
    }
    [SerializationConstructor] private MinorTurnOrders(int tick, EntityRef<Regime> regime) 
        : base(tick, regime)
    {
    }
}
