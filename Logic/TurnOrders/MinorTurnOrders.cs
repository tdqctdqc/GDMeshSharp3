using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class MinorTurnOrders : RegimeTurnOrders
{
    public MilitaryMinTurnOrders Military { get; private set; }
    public DiplomacyMinTurnOrders Diplomacy { get; private set; }
    public static MinorTurnOrders Construct(int tick, Regime regime)
    {
        return new MinorTurnOrders(tick, regime.MakeRef(), 
            MilitaryMinTurnOrders.Construct(),
            DiplomacyMinTurnOrders.Construct());
    }
    [SerializationConstructor] private MinorTurnOrders(int tick, 
        EntityRef<Regime> regime,
        MilitaryMinTurnOrders military,
        DiplomacyMinTurnOrders diplomacy) 
        : base(tick, regime)
    {
        Military = military;
        Diplomacy = diplomacy;
    }
}
