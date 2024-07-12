using System;
using System.Collections.Generic;
using System.Linq;

public class TownHall : SettlementBuilding
{
    public TownHall(Items items, PeepJobList jobs, FlowList flows) 
        : base(nameof(TownHall), 
            new LaborComponent(
                IdCount<Item>.Construct(
                ), 
                IdCount<Item>.Construct(
                ), 
                IdCount<PeepJob>.Construct(
                    (jobs.Bureaucrat, 100)
                )
            ),
            new MakeableAttribute(
                IdCount<Item>.Construct(
                    new Dictionary<Item, float>
                    {
                        { flows.ConstructionCap, 20_000 },
                    }),
                IdCount<Item>.Construct(
                    new Dictionary<Item, float> {})
                ))
    {
    }

    public override bool CanBuildInCell(Cell t, Data data)
    {
        return t is LandCell;
    }

    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand;
    }
}
