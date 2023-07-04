using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class ExtractionBuildingModel : ItemProdBuildingModel
{
    public ExtractionBuildingModel(Item prodItem, string name, bool fromDeposit, int numTicksToBuild, 
        int laborPerTickToBuild, int income, int prodCap) 
        : base(BuildingType.Extraction, prodItem, name, 
            numTicksToBuild, laborPerTickToBuild, income, prodCap)
    {
        if (prodItem.Attributes.Has<ExtractableAttribute>() == false) throw new Exception();
    }
    public override void Work(ProduceConstructProcedure proc, MapPolygon poly, float staffingRatio, 
        Data data)
    {
        staffingRatio = Mathf.Clamp(staffingRatio, 0f, 1f);
        var deposit = poly.GetResourceDeposits(data)
            .First(d => d.Item.Model() == ProdItem);
        var depSize = deposit.Size;
        var prod = Mathf.FloorToInt(staffingRatio * ProdCap);
        prod = Mathf.Min(Mathf.FloorToInt(depSize), prod);
        var rId = poly.Regime.RefId;
        
        proc.RegimeResourceGains[rId].Add(ProdItem, prod);
    }
}
