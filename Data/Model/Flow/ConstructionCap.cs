using System;
using System.Collections.Generic;
using System.Linq;

public class ConstructionCap : Flow
{
    public ConstructionCap() : base(nameof(ConstructionCap))
    {
    }

    public override float GetNonBuildingSupply(Regime r, Data d)
    {
        var val = d.Models.Flows.IndustrialPower.GetNonBuildingSupply(r, d)
                  + r.GetPopulation(d);
        return val / 10f;
    }
}
