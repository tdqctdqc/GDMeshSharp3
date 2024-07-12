using System;
using System.Collections.Generic;
using System.Linq;

public class Factory : SettlementBuilding
{
    public Factory(Items items, FlowList flows, PeepJobList jobs) : 
        base(nameof(Factory),
            new LaborComponent(
                IdCount<Item>.Construct(
                ),
                IdCount<Item>.Construct(
                    (flows.IndustrialPower, 100)
                ),
                IdCount<PeepJob>.Construct(
                    (jobs.Prole, 500)
                )
            ),
            new MakeableAttribute(IdCount<Item>.Construct(
                    new Dictionary<Item, float>
                {
                    { items.Iron, 500 },
                    { flows.ConstructionCap, 100_000 },
                }),
                IdCount<Item>.Construct(
                    new Dictionary<Item, float>
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
