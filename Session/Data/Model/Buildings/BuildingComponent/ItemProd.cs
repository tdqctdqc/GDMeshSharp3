using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ItemProd : BuildingComponent
{
    public Item ProdItem { get; private set; }
    public int ProdCap { get; private set; }

    public ItemProd(Item prodItem, int prodCap)
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
        var wallet = proc.RegimeResourceProds[rId];
        wallet.Add(ProdItem, prod);
    }
}
