using System;
using System.Collections.Generic;
using System.Linq;

public class TownHall : SettlementBuildingModel
{
    public TownHall(Items items, PeepJobList jobs, FlowList flows) 
        : base(nameof(TownHall), 
            new List<BuildingModelComponent>
            {
                new LaborComponent(
                    IdCount<IModel>.Construct(
                    ), 
                    IdCount<IModel>.Construct(
                    ), 
                    IdCount<PeepJob>.Construct(
                        (jobs.Bureaucrat, 100)
                    )
                )
            },
            new MakeableAttribute(
                IdCount<IModel>.Construct(
                    new Dictionary<IModel, float>
                    {
                        { flows.ConstructionCap, 20_000 },
                    }),
                IdCount<IModel>.Construct(
                    new Dictionary<IModel, float> {})
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
