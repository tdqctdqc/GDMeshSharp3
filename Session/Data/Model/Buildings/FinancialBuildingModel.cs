using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FinancialBuildingModel : WorkBuildingModel
{
    public FinancialBuildingModel(int income, string name, int numTicksToBuild, int constructionCapPerTick) 
        : base(BuildingType.Financial, name, numTicksToBuild, 
            constructionCapPerTick, income)
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
    public override Dictionary<PeepJob, int> JobLaborReqs { get; }
    public override void Work(ProduceConstructProcedure proc, MapPolygon poly, float staffingRatio, Data data)
    {
        proc.RegimeResourceGains[poly.Regime.RefId].Add(ItemManager.FinancialPower, Mathf.CeilToInt(Income * staffingRatio));
    }
}
