
using System.Collections.Generic;

public class HandleUnitOrdersModule : LogicModule
{
    public override LogicResults Calculate(List<RegimeTurnOrders> orders, Data data)
    {
        var res = new LogicResults();
        
        var proc = new HandleUnitOrdersProcedure();
        foreach (var group in data.GetAll<UnitGroup>())
        {
            group.Order.Handle(group, data, proc);
        }
        
        res.Messages.Add(proc);
        return res;
    }
}