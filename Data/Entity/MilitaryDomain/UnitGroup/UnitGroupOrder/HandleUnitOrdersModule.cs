
using System;
using System.Collections.Generic;

public class HandleUnitOrdersModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, Data data,
        Action<Message> sendMessage)
    {
        var proc = HandleUnitOrdersProcedure.Construct();
        foreach (var group in data.GetAll<UnitGroup>())
        {
            group.Order.Handle(group, data, proc);
        }
        sendMessage(proc);
    }
}