using System;
using System.Collections.Generic;
using System.Linq;

public class Bank : SettlementBuildingModel
{
    public Bank(Items items, PeepJobList jobs, FlowList flows) 
        : base(nameof(Bank), 
            new List<BuildingModelComponent>
            {
                new BuildingProd(
                    IdCount<IModel>.Construct(
                        (flows.Labor, 500)
                    ),
                    
                    IdCount<IModel>.Construct(
                        (flows.Income, 100)
                    ), 
                    
                    IdCount<PeepJob>.Construct(
                        (jobs.Bureaucrat, 500)
                    ), 
                    
                    flows)
            },
            new MakeableAttribute(
                IdCount<IModel>.Construct(new Dictionary<IModel, float>
                {
                    { items.FinancialPower, 10_000 },
                    { flows.ConstructionCap, 5_000 },
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
