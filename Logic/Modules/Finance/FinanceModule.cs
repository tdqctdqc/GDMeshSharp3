using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FinanceModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders,
        LogicWriteKey key)
    {
        var proc = GrowFinancialPowerProcedure.Construct();
        foreach (var regime in key.Data.GetAll<Regime>())
        {
            //todo get from 'flow dic' on regime or something
            var income = regime.Store.Get(key.Data.Models.Flows.Income);
            income = Mathf.Clamp(income / 5f, 0f, float.MaxValue);
            proc.GrowthsByRegimeId.Add(regime.Id, Mathf.FloorToInt(income));
        }

        key.SendMessage(proc);
    }
}
