
using System.Collections.Generic;

public interface IBudgetPriority
{
    string Name { get; }
    BudgetAccount Account { get; }
    float Weight { get; }
    Dictionary<Item, int> Wishlist { get; }
    void SetWeight(Data data, Regime regime);
    void Wipe();
    void SetWishlist(Regime r, Data d, BudgetPool pool, float proportion);
    void FirstRound(MajorTurnOrders orders, Regime regime, 
        float proportion, BudgetPool pool, Data data);

    void SecondRound(MajorTurnOrders orders, Regime regime, float proportion,
        BudgetPool pool, Data data, float multiplier);
}