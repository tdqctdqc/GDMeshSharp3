using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class MajorTurnOrders : RegimeTurnOrders
{
    public StartConstructionsOrders StartConstructions { get; private set; }
    public TradeOrders TradeOrders { get; private set; }
    public DiplomacyOrders Diplomacy { get; private set; }
    public ManufacturingOrders Manufacturing { get; private set; }
    public MilitaryMajTurnOrders Military { get; private set; }
    public AllianceMajorTurnOrders Alliance { get; private set; }
    public static MajorTurnOrders Construct(int tick, Regime regime)
    {
        return new MajorTurnOrders(tick, regime.MakeRef(), StartConstructionsOrders.Construct(),
            TradeOrders.Construct(), DiplomacyOrders.Construct(), 
            ManufacturingOrders.Construct(), MilitaryMajTurnOrders.Construct(),
            AllianceMajorTurnOrders.Construct());
    }
    [SerializationConstructor] private MajorTurnOrders(int tick, EntityRef<Regime> regime,
        StartConstructionsOrders startConstructions, TradeOrders tradeOrders, 
        DiplomacyOrders diplomacy, 
        ManufacturingOrders manufacturing, 
        MilitaryMajTurnOrders military,
        AllianceMajorTurnOrders alliance) 
        : base(tick, regime)
    {
        StartConstructions = startConstructions;
        TradeOrders = tradeOrders;
        Diplomacy = diplomacy;
        Manufacturing = manufacturing;
        Military = military;
        Alliance = alliance;
    }
}
