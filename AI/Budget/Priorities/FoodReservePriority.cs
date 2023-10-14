using System;
using System.Collections.Generic;
using System.Linq;

public class FoodReservePriority : ReservePriority
{
    public FoodReservePriority() 
        : base("Food Reserve")
    {
    }
    public override Dictionary<Item, int> CalculateWishlist(Regime regime, Data data, 
        BudgetPool pool, float proportion)
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