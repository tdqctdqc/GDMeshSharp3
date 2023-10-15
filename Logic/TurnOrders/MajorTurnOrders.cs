using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class MajorTurnOrders : RegimeTurnOrders
{
    public StartConstructionsOrders StartConstructions { get; private set; }
    public TradeOrders TradeOrders { get; private set; }
    public DiplomacyOrders DiplomacyOrders { get; private set; }
    public ManufacturingOrders ManufacturingOrders { get; private set; }
    public MilitaryOrders MilitaryOrders { get; private set; }
    public AllianceTurnOrders AllianceOrders { get; private set; }
    public static MajorTurnOrders Construct(int tick, Regime regime)
    {
        return new MajorTurnOrders(tick, regime.MakeRef(), StartConstructionsOrders.Construct(),
            TradeOrders.Construct(), DiplomacyOrders.Construct(), 
            ManufacturingOrders.Construct(), MilitaryOrders.Construct(),
            AllianceTurnOrders.Construct());
    }
    [SerializationConstructor] private MajorTurnOrders(int tick, EntityRef<Regime> regime,
        StartConstructionsOrders startConstructions, TradeOrders tradeOrders, 
        DiplomacyOrders diplomacyOrders, 
        ManufacturingOrders manufacturingOrders, 
        MilitaryOrders militaryOrders,
        AllianceTurnOrders allianceOrders) 
        : base(tick, regime)
    {
        StartConstructions = startConstructions;
        TradeOrders = tradeOrders;
        DiplomacyOrders = diplomacyOrders;
        ManufacturingOrders = manufacturingOrders;
        MilitaryOrders = militaryOrders;
        AllianceOrders = allianceOrders;
    }
}
