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
                new BuildingProd(
                    IdCount<IModel>.Construct(
                        (flows.Labor, 500)
                    ),
                    IdCount<IModel>.Construct(
                        (prodItem, 20)
                    ),
                    IdCount<PeepJob>.Construct(
                        (jobs.Miner, 500)
                    ),
                    flows
                )
            },
            new MakeableAttribute(
                IdCount<IModel>.Construct(new Dictionary<IModel, float>
                {
                    {items.Iron, 1000},
                }),
                IdCount<IModel>.Construct(new Dictionary<IModel, float>
                {
                }))
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
               && ds.Any(d => d.Item.Get(data) == MinedItem);
    }
}
