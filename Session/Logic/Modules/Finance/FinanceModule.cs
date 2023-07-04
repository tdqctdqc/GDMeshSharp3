using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FinanceModule : LogicModule
{
    public override LogicResults Calculate(List<TurnOrders> orders, Data data)
    {
        var res = new LogicResults();
        var proc = GrowFinancialPowerProcedure.Construct();
        foreach (var regime in data.Society.Regimes.Entities)
        {
            //todo get from 'flow dic' on regime or something
            var income = regime.FlowCount[FlowManager.Income];
            proc.GrowthsByRegimeId.Add(regime.Id, Mathf.FloorToInt(income));
        }

        res.Procedures.Add(proc);
        return res;
    }
}
