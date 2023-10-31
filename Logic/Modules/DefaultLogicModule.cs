using System;
using System.Collections.Generic;
using System.Linq;

public class DefaultLogicModule : LogicModule
{
    private Func<Procedure> _func;

    public DefaultLogicModule(Func<Procedure> func)
    {
        _func = func;
    }

    public override void Calculate(List<RegimeTurnOrders> orders, LogicWriteKey key)
    {
        key.SendMessage(_func());
    }
}
