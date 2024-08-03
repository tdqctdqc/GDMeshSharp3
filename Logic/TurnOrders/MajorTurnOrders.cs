using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class MajorTurnOrders : RegimeTurnOrders
{
    public static MajorTurnOrders Construct(int tick, Regime regime)
    {
        return new MajorTurnOrders(tick, regime.MakeRef());
    }
    [SerializationConstructor] private MajorTurnOrders(int tick, ERef<Regime> regime) 
        : base(tick, regime)
    {
    }
}
