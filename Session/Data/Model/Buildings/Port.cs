using System.Collections.Generic;
using System.Linq;
using Godot;


public class Port : BuildingModel
{
    public Port(PeepJobList jobs, Items items) 
        : base(BuildingType.Infrastructure, nameof(Port), 
            100, 2000, 
            200, 
            new List<BuildingModelComponent>
            {
                new Workplace(new Dictionary<PeepJob, int>
                {
                    {jobs.Prole, 500}
                })
            }, 
            new Dictionary<Item, int>
            {
                {items.Iron, 2000}
            })
    {
    }

    protected override bool CanBuildInTriSpec(PolyTri t, Data data)
    {
        return t.Landform(data).IsLand && t.Landform(data).MinRoughness <= data.Models.Landforms.Hill.MinRoughness;
    }

    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand && p.Neighbors.Items(data).Any(n => n.IsWater());
    }
}