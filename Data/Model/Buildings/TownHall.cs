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
                new Workplace(new Dictionary<PeepJob, int>
                {
                    {jobs.Bureaucrat, 100}
                })
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
