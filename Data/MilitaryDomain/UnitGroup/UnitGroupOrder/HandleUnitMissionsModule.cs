
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class HandleUnitMissionsModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, 
        LogicWriteKey key)
    {
        var data = key.Data;
        var proc = HandleUnitMissionsProcedure.Construct();
        Parallel.ForEach(data.GetAll<Army>(), 
            group =>
            {
                group.LineMission.Handle(group, key, proc);
                foreach (var order in group.OtherOrders)
                {
                    order.Handle(group, key, proc);
                }
            }
        );
        key.SendMessage(proc);
    }
}