using System;
using System.Collections.Generic;
using System.Linq;

public class Factory : SettlementBuildingModel
{
    public Factory(Items items, FlowList flows, PeepJobList jobs) : 
        base(nameof(Factory),
        new List<BuildingModelComponent>
            {
                new LaborComponent(
                    IdCount<IModel>.Construct(
                    ),
                    IdCount<IModel>.Construct(
                        (flows.IndustrialPower, 100)
                    ),
                    IdCount<PeepJob>.Construct(
                        (jobs.Prole, 500)
                    )
                )
            },
            new MakeableAttribute(IdCount<IModel>.Construct(
                    new Dictionary<IModel, float>
                {
                    { items.Iron, 500 },
                    { flows.ConstructionCap, 100_000 },
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
