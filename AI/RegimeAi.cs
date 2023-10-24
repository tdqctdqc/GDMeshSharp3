
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

    public RegimeTurnOrders GetTurnOrders(Data data)
    {
        var major = data.BaseDomain.GameClock.MajorTurn(data);
        return major ? GetMajorTurnOrders(data) : GetMinorTurnOrders(data);
    }
    private MajorTurnOrders GetMajorTurnOrders(Data data)
    {
        var orders = MajorTurnOrders.Construct(data.BaseDomain.GameClock.Tick, Regime);
        var alliance = Regime.GetAlliance(data);
        var allianceLeader = alliance.Leader.Entity(data);
        if (allianceLeader == Regime)
        {
            data.HostLogicData.AllianceAis[alliance].Calculate(orders.Alliance, data);
        }
        
        Budget.Calculate(data, orders);
        Diplomacy.Calculate(data, orders);
        Military.CalculateMajor(data, orders);
        
        return orders; 
    }
    private MinorTurnOrders GetMinorTurnOrders(Data data)
    {
        var orders = MinorTurnOrders.Construct(data.BaseDomain.GameClock.Tick, Regime);
        Military.CalculateMinor(data, orders);
        return orders; 
    }
}
