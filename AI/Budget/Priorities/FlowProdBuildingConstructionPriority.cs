using System;
using System.Collections.Generic;
using System.Linq;

public class FlowProdBuildingConstructionPriority : ConstructionPriority
{
    public Flow ProducedFlow { get; private set; }
    public FlowProdBuildingConstructionPriority(
        Flow producedFlow, Func<Data, Regime, float> getWeight) 
        : base(
            $"{producedFlow.Name} Flow Prod",
            getWeight, 
            b => b.Components.Any(c => c is FlowProd p && p.ProdFlow == producedFlow),
            b => ((FlowProd)b.Components.First(c => c is FlowProd p && p.ProdFlow == producedFlow)).ProdCap)
    {
        ProducedFlow = producedFlow;
    }
}
