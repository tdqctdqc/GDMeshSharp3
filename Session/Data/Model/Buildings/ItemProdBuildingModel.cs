
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class ItemProdBuildingModel : WorkBuildingModel
{
    public Item ProdItem { get; private set; }
    public int ProdCap { get; private set; }
    protected ItemProdBuildingModel(BuildingType buildingType, Item prodItem, string name, int numTicksToBuild, 
        int constructionCapPerTick, int income, int prodCap)
        : base(buildingType, name, numTicksToBuild, constructionCapPerTick,
            income)
    {
        ProdItem = prodItem;
        ProdCap = prodCap;
    }

    public override void Work(ProduceConstructProcedure proc, MapPolygon poly, float staffingRatio, 
        Data data)
    {
        staffingRatio = Mathf.Clamp(staffingRatio, 0f, 1f);
        var prod = Mathf.FloorToInt(staffingRatio * ProdCap);
        var rId = poly.Regime.RefId;
        var wallet = proc.RegimeResourceGains[rId];
        wallet.Add(ProdItem, prod);
    }
}
