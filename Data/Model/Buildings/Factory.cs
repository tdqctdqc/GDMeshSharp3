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
                new BuildingProd(flows.IndustrialPower, 100),
                new Workplace(new Dictionary<PeepJob, int>
                {
                    {jobs.Prole, 500}
                })
            },
            new MakeableAttribute(IdCount<IModel>.Construct(
                    new Dictionary<IModel, float>
                {
                    { items.Iron, 500 },
                }),
                IdCount<IModel>.Construct(
                    new Dictionary<IModel, float>
                    {
                    })
            )
        )
    {
        
    }

    public override bool CanBuildInCell(Cell t, Data data)
    {
        return t is LandCell && t.GetLandform(data).MinRoughness <= data.Models.Landforms.Hill.MinRoughness;
    }

    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand;
    }
}
