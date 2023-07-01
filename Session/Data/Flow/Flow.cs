using System;
using System.Collections.Generic;
using System.Linq;

public abstract class Flow
{
    public static Income Income { get; private set; } = new ();
    public abstract float GetFlow(Regime r, Data d);
    public abstract float GetFlowProjection(Regime r, Data d, TurnOrders t);
}
