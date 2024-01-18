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
                IdCount<Item>.Construct(
                    new Dictionary<Item, float>
                    {
                        {items.BuildingMaterial, 500}
                    }), 
                0f))
    {
    }

    protected override bool CanBuildInCell(PolyCell t, Data data)
    {
        return t is LandCell;
    }

    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand;
    }
}
