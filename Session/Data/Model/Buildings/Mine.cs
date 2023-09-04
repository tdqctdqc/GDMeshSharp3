using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Mine : BuildingModel
{
    public NaturalResource MinedItem { get; private set; }
    public Mine(string name, NaturalResource prodItem, Items items, PeepJobList jobs, FlowList flows) 
        : base(BuildingType.Extraction, name, 
            150, 3000,
            new List<BuildingModelComponent>
            {
                new ExtractionProd(prodItem, 20),
                new Workplace(new Dictionary<PeepJob, int>
                {
                    {jobs.Miner, 500}
                })
            },
            new Dictionary<Item, int>
            {
                {items.Iron, 1000}
            })
    {
        MinedItem = prodItem;
        if (prodItem.Attributes.Has<MineableAttribute>() == false) throw new Exception();
    }

    protected override bool CanBuildInTriSpec(PolyTri t, Data data) => CanBuildInTri(t, data);
    public static bool CanBuildInTri(PolyTri t, Data data)
    {
        return t.Landform(data).IsLand;
    }
    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        var ds = p.GetResourceDeposits(data);
        return ds != null && ds.Any(d => d.Item.Model(data) == MinedItem);
    }
}
