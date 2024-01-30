using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class MajorTurnOrders : RegimeTurnOrders
{
    // public StartConstructionsOrders StartConstructions { get; private set; }
    public TradeOrders TradeOrders { get; private set; }
    public DiplomacyMajTurnOrders Diplomacy { get; private set; }
    public static MajorTurnOrders Construct(int tick, Regime regime)
    {
        return new MajorTurnOrders(tick, regime.MakeRef(), 
            TradeOrders.Construct(), 
            DiplomacyMajTurnOrders.Construct());
    }
    [SerializationConstructor] private MajorTurnOrders(int tick, ERef<Regime> regime,
        TradeOrders tradeOrders, 
        DiplomacyMajTurnOrders diplomacy) 
        : base(tick, regime)
    {
        TradeOrders = tradeOrders;
        Diplomacy = diplomacy;
    }
}
