using System.Collections.Generic;
using Godot;

public class Barracks : BuildingModel
{
    public Barracks(Items items, FlowList flows, PeepJobList jobs) 
        : base(BuildingType.Military, 
            nameof(Barracks), 10,
            500, 
            new List<BuildingModelComponent>
            {
                new BuildingProd(items.Recruits, 100),
                new BuildingProd(flows.MilitaryCap, 1000)
            }, 
            new MakeableAttribute(
                IdCount<IModel>.Construct(new Dictionary<IModel, float>
                {
                }),
                IdCount<IModel>.Construct(new Dictionary<IModel, float>
                {
                }))
        )
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