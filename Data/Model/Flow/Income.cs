using System;
using System.Collections.Generic;
using System.Linq;

public class Income : Flow
{

    public Income() : base(nameof(Income))
    {
    }

    public override float GetNonBuildingSupply(Regime r, Data d)
    {
        var fromPeeps = r.GetCells(d)
            .Select(p => p.GetPeep(d))
            .Select(p => p.Employment)
            .Sum(p => p.Counts
                .Sum(kvp => ((PeepJob)d.Models[kvp.Key]).Income * kvp.Value));
        var tradeBalance = r.Finance.LastTradeBalance;
        return fromPeeps + tradeBalance;
    }

}
