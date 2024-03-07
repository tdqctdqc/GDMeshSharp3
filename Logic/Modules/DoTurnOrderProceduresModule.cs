
using System.Collections.Generic;
using Godot;

public class DoTurnOrderProceduresModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, 
        LogicWriteKey key)
    {
        GD.Print("doing turn order procs");
        GD.Print("order count " + orders.Count);
        for (var i = 0; i < orders.Count; i++)
        {
            var regimeOrders = orders[i];
            if (regimeOrders == null) continue;
            for (var j = 0; j < regimeOrders.Procedures.Count; j++)
            {
                GD.Print("handling procedure");
                var proc = regimeOrders.Procedures[j];
                key.SendMessage(proc);
            }
        }
    }
}