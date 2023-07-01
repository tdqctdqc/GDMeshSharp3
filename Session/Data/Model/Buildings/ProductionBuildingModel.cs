
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class ProductionBuildingModel : WorkBuildingModel
{
    public Item ProdItem { get; private set; }
    public abstract int ProductionCap { get; }
    public override int Capacity => ProductionCap;
    protected ProductionBuildingModel(BuildingType buildingType, Item prodItem, string name, int numTicksToBuild, 
        int laborPerTickToBuild, int income)
        : base(buildingType, name, numTicksToBuild, laborPerTickToBuild,
            income)
    {
        ProdItem = prodItem;
    }

    public override void Produce(WorkProdConsumeProcedure proc, MapPolygon poly, float staffingRatio, 
        int ticksSinceLast, Data data)
    {
        staffingRatio = Mathf.Clamp(staffingRatio, 0f, 1f);
        var prod = Mathf.FloorToInt(staffingRatio * ProductionCap * ticksSinceLast);
        var rId = poly.Regime.RefId;
        var wallet = proc.RegimeResourceGains[rId];
        wallet.Add(ProdItem, prod);
    }
}
