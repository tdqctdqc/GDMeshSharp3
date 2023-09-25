using System;
using System.Collections.Generic;
using System.Linq;

public class Factory : BuildingModel
{
    public Factory(Items items, FlowList flows, PeepJobList jobs) : 
        base(BuildingType.Industry, nameof(Factory),
        20, 2000,
        new List<BuildingModelComponent>
            {
                new FlowProd(100, flows.IndustrialPower),
                new Workplace(new Dictionary<PeepJob, int>
                {
                    {jobs.Prole, 500}
                })
            },
        new AttributeHolder<IModelAttribute>(
                new MakeableAttribute(new Dictionary<Item, int>
                    {
                        {items.Iron, 500},
                        {items.BuildingMaterial, 1000}
                    }, 0f
                )
            )
        )
    {
        
    }

    protected override bool CanBuildInTriSpec(PolyTri t, Data data)
    {
        return t.Landform(data).IsLand && t.Landform(data).MinRoughness <= data.Models.Landforms.Hill.MinRoughness;
    }

    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand;
    }
}
