using System;
using System.Collections.Generic;
using System.Linq;

public abstract class TurnOrders
{
    public EntityRef<Regime> Regime { get; private set; }
    public int Tick { get; private set; }

    public TurnOrders(int tick, EntityRef<Regime> regime)
    {
        Tick = tick;
        Regime = regime;
    }

    public abstract void WriteToResult(LogicResults r, Data d);

}
