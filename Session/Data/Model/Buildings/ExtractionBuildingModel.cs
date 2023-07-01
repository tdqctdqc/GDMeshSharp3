using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class ExtractionBuildingModel : ProductionBuildingModel
{
    public ExtractionBuildingModel(Item prodItem, string name, bool fromDeposit, int numTicksToBuild, int laborPerTickToBuild, int income) 
        : base(BuildingType.Extraction, prodItem, name, numTicksToBuild, laborPerTickToBuild, income)
    {
        if (prodItem.Attributes.Has<ExtractableAttribute>() == false) throw new Exception();
    }
    public override void Produce(WorkProdConsumeProcedure proc, MapPolygon poly, float staffingRatio, 
        int ticksSinceLast, Data data)
    {
        staffingRatio = Mathf.Clamp(staffingRatio, 0f, 1f);
        var deposit = poly.GetResourceDeposits(data)
            .First(d => d.Item.Model() == ProdItem);
        var depSize = deposit.Size;
        var prod = Mathf.FloorToInt(staffingRatio * ProductionCap);
        prod = Mathf.Min(Mathf.FloorToInt(depSize), prod);
        prod *= ticksSinceLast;
        var rId = poly.Regime.RefId;
        
        proc.RegimeResourceGains[rId].Add(ProdItem, prod);
    }
}
