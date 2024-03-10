
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
    void FirstRound(Regime regime, 
        float proportion, BudgetPool pool, LogicWriteKey key);

    void SecondRound(Regime regime, float proportion,
        BudgetPool pool, LogicWriteKey key, float multiplier);
}