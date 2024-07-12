using System;
using System.Collections.Generic;
using System.Linq;

public class Bank : SettlementBuilding
{
    public Bank(Items items, PeepJobList jobs, FlowList flows) 
        : base(nameof(Bank), 
            new LaborComponent(
                IdCount<Item>.Construct(
                ),
                    
                IdCount<Item>.Construct(
                    (flows.Income, 100)
                ), 
                    
                IdCount<PeepJob>.Construct(
                    (jobs.Bureaucrat, 500)
                )
            ),
            new MakeableAttribute(
                IdCount<Item>.Construct(new Dictionary<Item, float>
                {
                    { items.FinancialPower, 10_000 },
                    { flows.ConstructionCap, 5_000 },
                }),
              IdCount<Item>.Construct(new Dictionary<Item, float>
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
