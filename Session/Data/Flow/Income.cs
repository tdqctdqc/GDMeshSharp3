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
        var fromBuildings = r.Polygons.Items(d)
            .Where(p => p.GetBuildings(d) != null)
            .Sum(p =>
                {
                    var bs = p.GetBuildings(d);
                    if(bs == null) return 0;

                    return bs.Select(b => b.Model.Model(d)).Sum(b => b.Income);
                }
            );
        var fromAgriculture = r.Polygons.Items(d).Sum(p => p.PolyFoodProd.Income(d));
        var tradeBalance = r.Finance.LastTradeBalance;
        
        return fromBuildings + fromAgriculture + tradeBalance;
    }

    public override float GetConsumption(Regime r, Data d)
    {
        return 0f;
    }
}
