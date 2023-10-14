
using System.Collections.Generic;
using Godot;

public class RegimeAi
{
    public Regime Regime { get; private set; }
    public BudgetAi Budget { get; private set; }
    public DiplomacyAi Diplomacy { get; private set; }
    public MilitaryAi Military { get; private set; }
    public RegimeAi(Regime regime, Data data)
    {
        Regime = regime;
        Military = new MilitaryAi(regime);
        Budget = new BudgetAi(Military, data, regime);
        Diplomacy = new DiplomacyAi(regime);
    }

    public MajorTurnOrders GetMajorTurnOrders(Data data)
    {
        var orders = MajorTurnOrders.Construct(data.BaseDomain.GameClock.Tick, Regime);
        Budget.Calculate(data, orders);
        Diplomacy.Calculate(data, orders);
        Military.Calculate(data, orders);
        return orders; 
    }
    public MinorTurnOrders GetMinorTurnOrders(Data data)
    {
        var orders = MinorTurnOrders.Construct(data.BaseDomain.GameClock.Tick, Regime);

        return orders; 
    }
}
