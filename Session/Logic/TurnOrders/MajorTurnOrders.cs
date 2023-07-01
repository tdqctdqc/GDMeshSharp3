using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class MajorTurnOrders : TurnOrders
{
    public StartConstructionsOrders StartConstructions { get; private set; }
    public TradeOrders TradeOrders { get; private set; }

    public static MajorTurnOrders Construct(int tick, Regime regime)
    {
        return new MajorTurnOrders(tick, regime.MakeRef(), StartConstructionsOrders.Construct(),
            TradeOrders.Construct());
    }
    [SerializationConstructor] private MajorTurnOrders(int tick, EntityRef<Regime> regime,
        StartConstructionsOrders startConstructions, TradeOrders tradeOrders) 
        : base(tick, regime)
    {
        StartConstructions = startConstructions;
        TradeOrders = tradeOrders;
    }

    public override void WriteToResult(LogicResults r, Data d)
    {
        StartConstructions.WriteToResult(d, r);
        TradeOrders.WriteToResult(d, r);
    }
}
