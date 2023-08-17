using System;
using System.Collections.Generic;
using System.Linq;

public class TownHall : BuildingModel
{
    public TownHall(Items items, PeepJobList jobs) 
        : base(BuildingType.Government, nameof(TownHall), 
            50, 500, 0,
            new List<BuildingModelComponent>
            {
                new Workplace(new Dictionary<PeepJob, int>
                {
                    {jobs.Bureaucrat, 100}
                })
            },new Dictionary<Item, int>
            {
                {items.Iron, 200}
            })
    {
    }

    protected override bool CanBuildInTriSpec(PolyTri t, Data data)
    {
        return t.Landform(data).IsLand;
    }

    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand;
    }
}
