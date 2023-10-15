using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FinanceModule : LogicModule
{
    public override LogicResults Calculate(List<RegimeTurnOrders> orders, Data data)
    {
        var res = new LogicResults();
        var proc = GrowFinancialPowerProcedure.Construct();
        foreach (var regime in data.GetAll<Regime>())
        {
            //todo get from 'flow dic' on regime or something
            var income = regime.Flows.Get(data.Models.Flows.Income).Net();
            income = Mathf.Clamp(income / 5f, 0f, float.MaxValue);
            proc.GrowthsByRegimeId.Add(regime.Id, Mathf.FloorToInt(income));
        }

        res.Messages.Add(proc);
        return res;
    }
}
