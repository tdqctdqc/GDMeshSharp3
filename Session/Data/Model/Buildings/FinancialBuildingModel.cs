using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FinancialBuildingModel : WorkBuildingModel
{
    public FinancialBuildingModel(int income, string name, int numTicksToBuild, int laborPerTickToBuild) 
        : base(BuildingType.Financial, name, numTicksToBuild, 
            laborPerTickToBuild, income)
    {
    }

    public override Dictionary<Item, int> BuildCosts { get; protected set; }
    protected override bool CanBuildInTriSpec(PolyTri t, Data data)
    {
        return t.Landform.IsLand;
    }

    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand;
    }

    public override int Capacity { get; }
    public override Dictionary<PeepJob, int> JobLaborReqs { get; }
    public override void Produce(WorkProdConsumeProcedure proc, MapPolygon poly, float staffingRatio, int ticksSinceLast, Data data)
    {
        proc.RegimeResourceGains[poly.Regime.RefId].Add(ItemManager.FinancialPower, Mathf.CeilToInt(Income * staffingRatio));
    }
}
