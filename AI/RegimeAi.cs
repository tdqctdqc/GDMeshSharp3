
using System.Collections.Generic;
using Godot;

public class RegimeAi
{
    public Regime Regime { get; private set; }
    public BudgetAi Budget { get; private set; }
    public RegimeMilitaryAi Military { get; private set; }
    public RegimeAi(Regime regime, Data data)
    {
        Regime = regime;
        Military = new RegimeMilitaryAi(regime, data);
        Budget = new BudgetAi(Military, data, regime);
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
        var allianceLeader = alliance.Leader.Get(key.Data);
        if (allianceLeader == Regime)
        {
            var ai = key.Data.HostLogicData.AllianceAis[alliance];
            ai.CalculateMajor(orders, alliance, key);
        }
        
        Budget.Calculate(key, orders);
        Military.CalculateMajor(key, orders);
        
        return orders; 
    }
    private MinorTurnOrders GetMinorTurnOrders(LogicWriteKey key)
    {
        var orders = MinorTurnOrders.Construct(key.Data.BaseDomain.GameClock.Tick, Regime);
        
        var alliance = Regime.GetAlliance(key.Data);
        var allianceLeader = alliance.Leader.Get(key.Data);
        if (allianceLeader == Regime)
        {
            var ai = key.Data.HostLogicData.AllianceAis[alliance];
            ai.CalculateMinor(key);
        }
        
        Military.CalculateMinor(key, orders);
        
        return orders; 
    }
}
