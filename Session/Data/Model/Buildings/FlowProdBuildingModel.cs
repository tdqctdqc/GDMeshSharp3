using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class FlowProdBuildingModel : WorkBuildingModel
{
    public int ProductionCap { get; private set; }
    public Flow Flow { get; private set; }
    public FlowProdBuildingModel(Flow flow, int productionCap, BuildingType buildingType, string name, int numTicksToBuild, 
        int laborPerTickToBuild, int income) 
        : base(buildingType, name, numTicksToBuild, 
            laborPerTickToBuild, income)
    {
        ProductionCap = productionCap;
        Flow = flow;
    }

    public override Dictionary<Item, int> BuildCosts { get; protected set; }
    public override Dictionary<PeepJob, int> JobLaborReqs { get; }
    public override void Work(ProduceConstructProcedure proc, MapPolygon poly, float staffingRatio, Data data)
    {
        staffingRatio = Mathf.Clamp(staffingRatio, 0f, 1f);
        var prod = Mathf.FloorToInt(staffingRatio * ProductionCap);
        var rId = poly.Regime.RefId;
        var wallet = proc.RegimeInflows[rId];
        wallet.Add(Flow, prod);
    }
}
