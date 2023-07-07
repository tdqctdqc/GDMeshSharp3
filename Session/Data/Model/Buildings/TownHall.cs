using System;
using System.Collections.Generic;
using System.Linq;

public class TownHall : BuildingModel
{
    public override Dictionary<Item, int> BuildCosts { get; protected set; }
        = new Dictionary<Item, int>
        {
            {ItemManager.Iron, 200}
        };
    public TownHall() 
        : base(BuildingType.Government, nameof(TownHall), 
            50, 500, 0,
            new List<BuildingComponent>
            {
                new Workplace(new Dictionary<PeepJob, int>
                {
                    {PeepJobManager.Bureaucrat, 100}
                })
            })
    {
    }

    protected override bool CanBuildInTriSpec(PolyTri t, Data data)
    {
        return t.Landform.IsLand;
    }

    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand;
    }
}
