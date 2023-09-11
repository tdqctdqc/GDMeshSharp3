using System;
using System.Collections.Generic;
using System.Linq;

public class FoodReservePriority : BudgetPriority
{
    public FoodReservePriority() 
        : base("Food Reserve", (r,d) => .5f)
    {
    }

    public override void Calculate(Regime regime, Data data, MajorTurnOrders orders, 
        HashSet<Item> usedItem, HashSet<Flow> usedFlow,
        ref bool usedLabor)
    {
        return;
    }

    public override Dictionary<Item, int> GetWishlist(Regime regime, Data data, int availLabor, int availConstructCap)
    {
        var food = data.Models.Items.Food;
        var foodCons = data.BaseDomain.Rules.FoodConsumptionPerPeepPoint;
        var pop = regime.GetPeeps(data).Sum(p => p.Size);
        var foodNeed = foodCons * pop;
        var reserveRatio = 1.5f;
        var toBuy = foodNeed - regime.Items.Get(food);
        if (toBuy <= 0) return new Dictionary<Item, int>();
        return new Dictionary<Item, int> { { food, foodNeed } };
    }
}