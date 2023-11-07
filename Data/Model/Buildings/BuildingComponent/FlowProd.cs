using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FlowProd : BuildingModelComponent
{
    public int ProdCap { get; private set; }
    public Flow ProdFlow { get; private set; }

    public FlowProd(int prodCap, Flow prodFlow)
    {
        ProdCap = prodCap;
        ProdFlow = prodFlow;
    }

    public override void Work(ProduceConstructProcedure proc, MapPolygon poly, float staffingRatio, Data data)
    {
        staffingRatio = Mathf.Clamp(staffingRatio, 0f, 1f);
        var prod = Mathf.FloorToInt(staffingRatio * ProdCap);
        var rId = poly.OwnerRegime.RefId;
        var wallet = proc.RegimeFlows[rId];
        wallet.AddFlowIn(ProdFlow, prod);
    }
}
