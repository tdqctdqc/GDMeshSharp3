
using System.Collections.Generic;
using Godot;

public class RegimeAi
{
    public ERef<Regime> Regime { get; private set; }
    public BudgetAi Budget { get; private set; }
    public RegimeMilitaryAi Military { get; private set; }
    public RegimeTechnologyAi Technology { get; private set; }
    
    public List<string> Status { get; private set; }

    public static RegimeAi Construct(Regime r, Data d)
    {
        return new RegimeAi(
            r.MakeRef(),
            BudgetAi.Construct(r, d),
            new RegimeMilitaryAi(r, d),
            new RegimeTechnologyAi(r, d),
            new List<string>()
        );
    }
    public RegimeAi(ERef<Regime> regime, BudgetAi budget, RegimeMilitaryAi military, RegimeTechnologyAi technology, List<string> status)
    {
        Regime = regime;
        Budget = budget;
        Military = military;
        Technology = technology;
        Status = status;
    }

    public RegimeTurnOrders CalculateAndSendOrders(LogicKey key)
    {
        Status.Clear();
        var major = key.Data.BaseDomain.GameClock.MajorTurn(key.Data);
        RegimeTurnOrders orders = major ? GetMajorTurnOrders(key) : GetMinorTurnOrders(key);
        Status.Add("Finished");
        return orders;
    }
    private MajorTurnOrders GetMajorTurnOrders(LogicKey key)
    {
        Status.Add("Doing major");
        var regime = Regime.Get(key.Data);
        var orders = MajorTurnOrders.Construct(key.Data.BaseDomain.GameClock.Tick, regime);
        var alliance = regime.GetAlliance(key.Data);
        var allianceLeader = alliance.Leader.Get(key.Data);
        if (allianceLeader == regime)
        {
            var ai = key.Data.HostLogicData.AllianceAis[alliance];
            ai.CalculateMajor(orders, alliance, key);
        }
        
        Military.CalculateMajor(key, orders);
        Technology.Calculate(key);
        Budget.Calculate(key, orders);
        Status.RemoveAt(Status.Count - 1);

        return orders; 
    }
    private MinorTurnOrders GetMinorTurnOrders(LogicKey key)
    {
        var regime = Regime.Get(key.Data);
        var orders = MinorTurnOrders.Construct(key.Data.BaseDomain.GameClock.Tick, regime);

        var alliance = regime.GetAlliance(key.Data);
        var allianceLeader = alliance.Leader.Get(key.Data);
        if (allianceLeader == regime)
        {
            var ai = key.Data.HostLogicData.AllianceAis[alliance];
            ai.CalculateMinor(key);
        }
        
        Military.CalculateMinor(key, orders);
        
        return orders; 
    }
}
