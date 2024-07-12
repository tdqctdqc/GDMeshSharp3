using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Mine : ResourceExtractionBuilding
{
    public Mine(string name, NaturalResource prodItem, 
        Items items, PeepJobList jobs, FlowList flows) 
        : base(name, prodItem, 20, 500, 
            100, jobs.Miner,
            new MakeableAttribute(
                IdCount<Item>.Construct(new Dictionary<Item, float>
                {
                    {items.Iron, 1000},
                    { flows.ConstructionCap, 200_000 },
                }),
                IdCount<Item>.Construct(new Dictionary<Item, float>
                {
                }))
        )
    {
    }
    public override bool CanBuildInCell(Cell t, Data data)
    {
        return t is LandCell
            && data.Planet.ResourceDepositAux.ByCell[t].Item
                .Get(data) == Resource(data);
    }
}
