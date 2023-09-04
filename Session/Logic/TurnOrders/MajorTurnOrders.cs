using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class MajorTurnOrders : TurnOrders
{
    public StartConstructionsOrders StartConstructions { get; private set; }
    public TradeOrders TradeOrders { get; private set; }
    public DiplomacyOrders DiplomacyOrders { get; private set; }
    public ManufacturingOrders ManufacturingOrders { get; private set; }

    public static MajorTurnOrders Construct(int tick, Regime regime)
    {
        return new MajorTurnOrders(tick, regime.MakeRef(), StartConstructionsOrders.Construct(),
            TradeOrders.Construct(), DiplomacyOrders.Construct(), ManufacturingOrders.Construct());
    }
    [SerializationConstructor] private MajorTurnOrders(int tick, EntityRef<Regime> regime,
        StartConstructionsOrders startConstructions, TradeOrders tradeOrders, 
        DiplomacyOrders diplomacyOrders, ManufacturingOrders manufacturingOrders) 
        : base(tick, regime)
    {
        StartConstructions = startConstructions;
        TradeOrders = tradeOrders;
        DiplomacyOrders = diplomacyOrders;
        ManufacturingOrders = manufacturingOrders;
    }
}
