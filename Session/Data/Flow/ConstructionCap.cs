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
        return FlowManager.IndustrialPower.GetNonBuildingFlow(r, d) 
               + r.GetPopulation(d);
    }

    public override float GetConsumption(Regime r, Data d)
    {
        return d.Infrastructure.CurrentConstruction.ByPoly
            .Where(kvp => r.Polygons.RefIds.Contains(kvp.Key))
            .Sum(kvp => kvp.Value.Sum(c => c.Model.Model(d).ConstructionCapPerTick));
    }
}
