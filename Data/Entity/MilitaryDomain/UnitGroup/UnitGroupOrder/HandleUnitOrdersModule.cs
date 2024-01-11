
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
                group.GroupOrder.Handle(group, key, proc);
            }
        );
        key.SendMessage(proc);
    }
}