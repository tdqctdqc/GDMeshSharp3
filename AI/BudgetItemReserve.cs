using System;
using System.Collections.Generic;
using System.Linq;

public class BudgetItemReserve
{
    public Dictionary<Item, int> DesiredReserves { get; private set; }

    public BudgetItemReserve()
    {
        DesiredReserves = new Dictionary<Item, int>();
    }

    public void Calculate(Regime regime, Data data)
    {
        var foodCons = data.BaseDomain.Rules.FoodConsumptionPerPeepPoint;
        var pop = regime.GetPeeps(data).Sum(p => p.Size);
        var foodNeed = foodCons * pop;
        DesiredReserves[ItemManager.Food] = foodNeed;
    }
}
