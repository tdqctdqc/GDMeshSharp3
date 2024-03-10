using System;
using System.Collections.Generic;
using System.Linq;

public class FlowProdBuildingConstructionPriority : ConstructionPriority
{
    public Flow ProducedFlow { get; private set; }
    public FlowProdBuildingConstructionPriority(
        Flow producedFlow, Func<Data, Regime, float> getWeight) 
        : base($"{producedFlow.Name} Flow Prod",
            getWeight)
    {
        ProducedFlow = producedFlow;
    }

    protected override float Utility(BuildingModel t)
    {
        return ((FlowProd)t.Components.First(c => c is FlowProd p && p.ProdFlow == ProducedFlow)).ProdCap;
    }

    protected override bool Relevant(BuildingModel t, Data d)
    {
        return t.Components.Any(c => c is FlowProd p && p.ProdFlow == ProducedFlow);
    }
}
