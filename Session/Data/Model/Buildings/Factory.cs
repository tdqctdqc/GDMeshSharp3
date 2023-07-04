using System;
using System.Collections.Generic;
using System.Linq;

public class Factory : FlowProdBuildingModel
{
    public override Dictionary<PeepJob, int> JobLaborReqs { get; }
        = new Dictionary<PeepJob, int>
        {
            {PeepJobManager.Prole, 500}
        };
    public Factory() : base(ItemManager.IndustrialPower, 100, BuildingType.Industry, nameof(Factory),
        100, 2000, 10)
    {
    }

    public override Dictionary<Item, int> BuildCosts { get; protected set; }
        = new Dictionary<Item, int>
        {
            {ItemManager.Iron, 1200}
        };
    protected override bool CanBuildInTriSpec(PolyTri t, Data data)
    {
        return t.Landform.IsLand && t.Landform.MinRoughness <= LandformManager.Hill.MinRoughness;
    }

    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand && p.HasSettlement(data);
    }
}
