using System;
using System.Collections.Generic;
using System.Linq;

public abstract class RegimeTurnOrders
{
    public ERef<Regime> Regime { get; private set; }
    public int Tick { get; private set; }

    public RegimeTurnOrders(int tick, ERef<Regime> regime)
    {
        Tick = tick;
        Regime = regime;
    }


}
