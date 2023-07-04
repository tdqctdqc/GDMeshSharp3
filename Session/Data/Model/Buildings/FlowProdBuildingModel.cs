using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class FlowProdBuildingModel : WorkBuildingModel
{
    public int ProdCap { get; private set; }
    public Flow ProdFlow { get; private set; }
    public FlowProdBuildingModel(Flow prodFlow, int prodCap, BuildingType buildingType, string name, int numTicksToBuild, 
        int constructionCapPerTick, int income) 
        : base(buildingType, name, numTicksToBuild, 
            constructionCapPerTick, income)
    {
        ProdCap = prodCap;
        ProdFlow = prodFlow;
    }

    public override Dictionary<Item, int> BuildCosts { get; protected set; }
    public override Dictionary<PeepJob, int> JobLaborReqs { get; }
    public override void Work(ProduceConstructProcedure proc, MapPolygon poly, float staffingRatio, Data data)
    {
        staffingRatio = Mathf.Clamp(staffingRatio, 0f, 1f);
        var prod = Mathf.FloorToInt(staffingRatio * ProdCap);
        var rId = poly.Regime.RefId;
        var wallet = proc.RegimeInflows[rId];
        wallet.Add(ProdFlow, prod);
    }
}
