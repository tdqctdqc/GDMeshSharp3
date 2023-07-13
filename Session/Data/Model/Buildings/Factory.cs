using System;
using System.Collections.Generic;
using System.Linq;

public class Factory : BuildingModel
{
    public Factory() : base(BuildingType.Industry, nameof(Factory),100, 
        2000, 100,
        
        new List<BuildingComponent>
            {
                new FlowProd(100, FlowManager.IndustrialPower),
                new Workplace(new Dictionary<PeepJob, int>
                {
                    {PeepJobManager.Prole, 500}
                })
            })
    {
        
    }

    public override Dictionary<Item, int> BuildCosts { get; protected set; }
        = new Dictionary<Item, int>
        {
            {ItemManager.Iron, 1000}
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
