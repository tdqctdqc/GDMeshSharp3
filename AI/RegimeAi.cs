
using System.Collections.Generic;
using Godot;

public class RegimeAi
{
    public Regime Regime { get; private set; }
    public BudgetAi Budget { get; private set; }
    public DiplomacyAi Diplomacy { get; private set; }
    public RegimeMilitaryAi Military { get; private set; }
    public RegimeAi(Regime regime, Data data)
    {
        Regime = regime;
        Military = new RegimeMilitaryAi(regime);
        Budget = new BudgetAi(Military, data, regime);
        Diplomacy = new DiplomacyAi(regime);
    }

    public RegimeTurnOrders CalculateAndSendOrders(LogicWriteKey key)
    {
        var major = key.Data.BaseDomain.GameClock.MajorTurn(key.Data);
        return major ? GetMajorTurnOrders(key) : GetMinorTurnOrders(key);
    }
    private MajorTurnOrders GetMajorTurnOrders(LogicWriteKey key)
    {
        var orders = MajorTurnOrders.Construct(key.Data.BaseDomain.GameClock.Tick, Regime);
        var alliance = Regime.GetAlliance(key.Data);
        var allianceLeader = alliance.Leader.Entity(key.Data);
        if (allianceLeader == Regime)
        {
            var ai = key.Data.HostLogicData.AllianceAis[alliance];
            ai.Calculate(orders.Alliance, key);
        }
        
        Budget.Calculate(key, orders);
        Diplomacy.Calculate(key, orders);
        Military.CalculateMajor(key, orders);
        
        return orders; 
    }
    private MinorTurnOrders GetMinorTurnOrders(LogicWriteKey key)
    {
        var orders = MinorTurnOrders.Construct(key.Data.BaseDomain.GameClock.Tick, Regime);
        Military.CalculateMinor(key, orders);
        return orders; 
    }
}
