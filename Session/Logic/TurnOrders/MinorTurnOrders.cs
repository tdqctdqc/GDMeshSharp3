using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class MinorTurnOrders : TurnOrders
{
    public static MinorTurnOrders Construct(int tick, Regime regime)
    {
        return new MinorTurnOrders(tick, regime.MakeRef());
    }
    [SerializationConstructor] private MinorTurnOrders(int tick, EntityRef<Regime> regime) 
        : base(tick, regime)
    {
    }

    public override void WriteToResult(LogicResults r, Data d)
    {
        return;
    }
}
