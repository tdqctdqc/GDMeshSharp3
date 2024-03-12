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
                new BuildingProd(flows.Income, 100)
            },
            new MakeableAttribute(
                IdCount<IModel>.Construct(new Dictionary<IModel, float>
                {
                    { items.FinancialPower, 10_000 },
                }),
              IdCount<IModel>.Construct(new Dictionary<IModel, float>
                {
                })
            )  
        )
    {
    }
    public override bool CanBuildInCell(Cell t, Data data)
    {
        return t is LandCell;
    }

    public override bool CanBuildInPoly(MapPolygon p, Data data)
    {
        return p.IsLand;
    }
}
