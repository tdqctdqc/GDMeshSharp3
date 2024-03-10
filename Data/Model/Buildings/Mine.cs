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
            new MakeableAttribute(
                IdCount<Item>.Construct(new Dictionary<Item, float>
                {
                    {items.Iron, 1000},
                    {items.BuildingMaterial, 1000}
                }), 0f)
        )
    {
        MinedItem = prodItem;
        if (prodItem is IMineable == false) throw new Exception();
    }
    public override bool CanBuildInCell(Cell t, Data data)
    {
        return t is LandCell;
    }
    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        var ds = p.GetResourceDeposits(data);
        return ds != null 
               && ds.Any(d => d.Item.Model(data) == MinedItem);
    }
}
