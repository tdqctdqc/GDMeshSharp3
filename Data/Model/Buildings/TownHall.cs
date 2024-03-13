using System;
using System.Collections.Generic;
using System.Linq;

public class TownHall : BuildingModel
{
    public TownHall(Items items, PeepJobList jobs, FlowList flows) 
        : base(BuildingType.Government, nameof(TownHall), 
            50, 
            500, 
            new List<BuildingModelComponent>
            {
                new BuildingProd(
                    IdCount<IModel>.Construct(
                        (flows.Labor, 100)
                    ), 
                    IdCount<IModel>.Construct(
                    ), 
                    IdCount<PeepJob>.Construct(
                        (jobs.Bureaucrat, 100)
                    ),
                    flows
                )
            },
            new MakeableAttribute(
                IdCount<IModel>.Construct(
                    new Dictionary<IModel, float> {}),
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
