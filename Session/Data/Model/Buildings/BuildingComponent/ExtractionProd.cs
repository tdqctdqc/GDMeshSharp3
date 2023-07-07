using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ExtractionProd : ItemProd
{
    public NaturalResource NaturalResource => (NaturalResource) ProdItem;
    public ExtractionProd(NaturalResource prodItem, int prodCap) : base(prodItem, prodCap)
    {
    }
    public override void Work(ProduceConstructProcedure proc, MapPolygon poly, float staffingRatio, 
        Data data)
    {
        staffingRatio = Mathf.Clamp(staffingRatio, 0f, 1f);
        var deposit = poly.GetResourceDeposits(data)
            .First(d => d.Item.Model(data) == ProdItem);
        var depSize = deposit.Size;
        var prod = Mathf.FloorToInt(staffingRatio * ProdCap);
        prod = Mathf.Min(Mathf.FloorToInt(depSize), prod);
        var rId = poly.Regime.RefId;
        
        proc.RegimeResourceGains[rId].Add(ProdItem, prod);
    }
}
