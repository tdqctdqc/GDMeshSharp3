using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Mine : BuildingModel
{
    public NaturalResource MinedItem { get; private set; }
    public Mine(string name, NaturalResource prodItem) 
        : base(BuildingType.Extraction, name, 
            150, 3000,
            150, 
            new List<BuildingComponent>
            {
                new ExtractionProd(prodItem, 20),
                new Workplace(new Dictionary<PeepJob, int>
                {
                    {PeepJobManager.Miner, 500}
                })
            })
    {
        MinedItem = prodItem;
        if (prodItem.Attributes.Has<MineableAttribute>() == false) throw new Exception();
    }
    public override Dictionary<Item, int> BuildCosts { get; protected set; }
        = new Dictionary<Item, int>
        {
            {ItemManager.Iron, 1000}
        };


    protected override bool CanBuildInTriSpec(PolyTri t, Data data) => CanBuildInTri(t);
    public static bool CanBuildInTri(PolyTri t)
    {
        return t.Landform.IsLand;
    }
    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        var ds = p.GetResourceDeposits(data);
        return ds != null && ds.Any(d => d.Item.Model(data) == MinedItem);
    }
}
