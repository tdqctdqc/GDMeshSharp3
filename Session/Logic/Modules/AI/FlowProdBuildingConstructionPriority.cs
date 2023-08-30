using System;
using System.Collections.Generic;
using System.Linq;

public class FlowProdBuildingConstructionPriority : BuildingConstructionPriority
{
    public Flow ProducedFlow { get; private set; }
    public FlowProdBuildingConstructionPriority(
        Flow producedFlow, Func<Data, Regime, float> getWeight) 
        : base(
            $"{producedFlow.Name} Flow Prod",
            b => b.Components.Any(c => c is FlowProd p && p.ProdFlow == producedFlow),
            b => ((FlowProd)b.Components.First(c => c is FlowProd p && p.ProdFlow == producedFlow)).ProdCap,
            getWeight)
    {
        ProducedFlow = producedFlow;
    }
}
