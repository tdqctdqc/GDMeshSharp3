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

    public override LogicResults Calculate(List<TurnOrders> orders, Data data)
    {
        var res = new LogicResults();
        res.Messages.Add(_func());
        return res;
    }
}
