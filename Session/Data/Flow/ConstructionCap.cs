using System;
using System.Collections.Generic;
using System.Linq;

public class ConstructionCap : Flow
{
    public ConstructionCap() : base(nameof(ConstructionCap))
    {
    }

    public override float GetNonBuildingFlow(Regime r, Data d)
    {
        return FlowManager.IndustrialPower.GetNonBuildingFlow(r, d) + r.GetPopulation(d);
    }
}
