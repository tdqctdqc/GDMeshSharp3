using System;
using System.Collections.Generic;
using System.Linq;

public class Income : Flow
{

    public Income() : base(nameof(Income))
    {
    }

    public override float GetNonBuildingFlow(Regime r, Data d)
    {
        var fromBuildings = r.Polygons
            .Where(p => p.GetBuildings(d) != null)
            .Sum(p =>
                {
                    var bs = p.GetBuildings(d);
                    if(bs == null) return 0;

                    return bs.Select(b => b.Model.Model())
                        .SelectWhereOfType<BuildingModel, WorkBuildingModel>()
                        .Sum(b => b.Income);
                }
            );
        var fromAgriculture = r.Polygons.Sum(p => p.PolyFoodProd.Income(d));
        var tradeBalance = r.Finance.LastTradeBalance;
        
        return fromBuildings + fromAgriculture + tradeBalance;
    }
}
