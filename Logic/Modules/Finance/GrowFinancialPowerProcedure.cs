using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class GrowFinancialPowerProcedure : Procedure
{
    public Dictionary<int, int> GrowthsByRegimeId { get; private set; }

    public static GrowFinancialPowerProcedure Construct()
    {
        return new GrowFinancialPowerProcedure(new Dictionary<int, int>());
    }
    [SerializationConstructor] private GrowFinancialPowerProcedure(Dictionary<int, int> growthsByRegimeId)
    {
        GrowthsByRegimeId = growthsByRegimeId;
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var d = key.Data;
        foreach (var kvp in GrowthsByRegimeId)
        {
            var regime = (Regime) d[kvp.Key];
            regime.Items.Add(key.Data.Models.Items.FinancialPower, kvp.Value);
        }
    }
}
