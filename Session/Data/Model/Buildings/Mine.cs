using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Mine : ExtractionBuildingModel
{
    public Mine(string name, Item prodItem) 
        : base(prodItem, name, true, 
            150, 3000, 20)
    {
        if (prodItem.Attributes.Has<MineableAttribute>() == false) throw new Exception();
    }

    public override Dictionary<PeepJob, int> JobLaborReqs { get; }
        = new Dictionary<PeepJob, int>
        {
            {PeepJobManager.Miner, 500}
        };
    public override Dictionary<Item, int> BuildCosts { get; protected set; }
        = new Dictionary<Item, int>
        {
            {ItemManager.Iron, 1000}
        };

    public override int ProductionCap { get; } = 10;

    protected override bool CanBuildInTriSpec(PolyTri t, Data data) => CanBuildInTri(t);
    public static bool CanBuildInTri(PolyTri t)
    {
        return t.Landform.IsLand;
    }
    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        var ds = p.GetResourceDeposits(data);
        return ds != null && ds.Any(d => d.Item.Model() == ProdItem);
    }
}
