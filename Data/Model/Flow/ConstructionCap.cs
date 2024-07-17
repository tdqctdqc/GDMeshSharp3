using System;
using System.Collections.Generic;
using System.Linq;

public class ConstructionCap : Flow
{
    public ConstructionCap()
    {
    }

    public override float GetNonBuildingSupply(Regime r, Data d)
    {
        return r.GetPopulation(d);
        // var val = d.Models.Items.IndustrialPower.GetNonBuildingSupply(r, d)
        //           + r.GetPopulation(d);
        // return val / 10f;
    }
}
