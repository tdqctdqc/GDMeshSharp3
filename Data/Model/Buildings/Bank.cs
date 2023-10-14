using System;
using System.Collections.Generic;
using System.Linq;

public class Bank : BuildingModel
{
    public Bank(Items items, PeepJobList jobs, FlowList flows) 
        : base(BuildingType.Financial, nameof(Bank), 
            25, 200, 
            new List<BuildingModelComponent>
            {
                new Workplace(new Dictionary<PeepJob, int>
                    {
                        {jobs.Bureaucrat, 500}
                    }),
                new FlowProd(100, flows.Income)
            },
            new MakeableAttribute(
                IdCount<Item>.Construct(new Dictionary<Item, float>
                {
                    { items.FinancialPower, 10_000 },
                    { items.BuildingMaterial, 500 },
                }), 0f
            )  
        )
    {
    }
    protected override bool CanBuildInTriSpec(PolyTri t, Data data)
    {
        return t.Landform(data).IsLand;
    }

    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand;
    }
}
