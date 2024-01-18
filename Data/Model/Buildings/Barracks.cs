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
                new ItemProd(items.Recruits, 100)
            }, 
            new MakeableAttribute(
                IdCount<Item>.Construct(new Dictionary<Item, float>
                {
                    { items.BuildingMaterial, 500 }
                }), 0f)
        )
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