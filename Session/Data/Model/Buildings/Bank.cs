using System;
using System.Collections.Generic;
using System.Linq;

public class Bank : BuildingModel
{
    public override Dictionary<Item, int> BuildCosts { get; protected set; }
        = new Dictionary<Item, int>
        {
            {ItemManager.FinancialPower, 10_000}
        };
    public Bank() 
        : base( BuildingType.Financial, nameof(Bank), 
            25, 200, 100,
            new List<BuildingComponent>
            {
                new Workplace(new Dictionary<PeepJob, int>
                    {
                        {PeepJobManager.Bureaucrat, 500}
                    }),
                new ItemProd(ItemManager.FinancialPower, 100)
            })
    {
    }
    protected override bool CanBuildInTriSpec(PolyTri t, Data data)
    {
        return t.Landform.IsLand;
    }

    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand;
    }
}
