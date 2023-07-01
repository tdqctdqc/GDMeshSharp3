using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FinanceModule : LogicModule
{
    public override LogicResults Calculate(Data data)
    {
        var res = new LogicResults();
        var proc = GrowFinancialPowerProcedure.Construct();
        foreach (var regime in data.Society.Regimes.Entities)
        {
            var income = Flow.Income.GetFlow(regime, data);
            proc.GrowthsByRegimeId.Add(regime.Id, Mathf.FloorToInt(income));
        }

        res.Procedures.Add(proc);
        return res;
    }
}
