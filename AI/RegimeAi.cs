
using System.Collections.Generic;

public class RegimeAi
{
    public Regime Regime { get; private set; }
    public BudgetAi Budget { get; private set; }
    public RegimeAi(Regime regime, Data data)
    {
        Regime = regime;
        Budget = new BudgetAi(data, regime);
    }

    public MajorTurnOrders GetMajorTurnOrders(Data data)
    {
        var orders = MajorTurnOrders.Construct(data.BaseDomain.GameClock.Tick, Regime);
        Budget.Calculate(data, orders);
        return orders; 
    }
    public MinorTurnOrders GetMinorTurnOrders(Data data)
    {
        var orders = MinorTurnOrders.Construct(data.BaseDomain.GameClock.Tick, Regime);

        return orders; 
    }
}
