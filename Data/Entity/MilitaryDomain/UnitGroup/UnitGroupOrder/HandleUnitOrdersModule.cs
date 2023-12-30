
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class HandleUnitOrdersModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, 
        LogicWriteKey key)
    {
        var data = key.Data;
        var proc = HandleUnitOrdersProcedure.Construct();
        Parallel.ForEach(data.GetAll<UnitGroup>(), 
            group =>
            {
                group.Order.Handle(group, key, proc);
            }
        );
        key.SendMessage(proc);
    }
}