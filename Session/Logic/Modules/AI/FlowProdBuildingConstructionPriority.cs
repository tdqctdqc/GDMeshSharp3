using System;
using System.Collections.Generic;
using System.Linq;

public class FlowProdBuildingConstructionPriority : BuildingConstructionPriority
{
    public Flow ProducedFlow { get; private set; }
    public FlowProdBuildingConstructionPriority(Flow producedFlow, Func<Data, Regime, float> getWeight) 
        : base(b => b is FlowProdBuildingModel p && p.ProdFlow == producedFlow,
            b => ((FlowProdBuildingModel)b).ProdCap,
            getWeight)
    {
        ProducedFlow = producedFlow;
    }
}
