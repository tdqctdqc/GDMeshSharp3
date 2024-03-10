

using System.Collections.Generic;

public abstract class ReservePriority : IBudgetPriority
{
    public string Name { get; }
    public BudgetAccount Account { get; private set; }
    public float Weight { get; private set; }
    public Dictionary<Item, int> Wishlist { get; private set; }

    protected ReservePriority(string name)
    {
        Name = name;
        Weight = 0f;
        Wishlist = new Dictionary<Item, int>();
        Account = new BudgetAccount();
    }

    public void SetWeight(Data data, Regime regime)
    {
        Weight = 1f;
    }
    public abstract Dictionary<Item, int> CalculateWishlist(Regime regime, Data data,
        BudgetPool pool, float proportion);
    public void Wipe()
    {
        Account.Clear();
        Wishlist.Clear();
    }

    public void SetWishlist(Regime r, Data d, BudgetPool pool, float proportion)
    {
        Wishlist = CalculateWishlist(r, d, pool, proportion);
    }

    public void FirstRound(Regime regime, float proportion, BudgetPool pool, LogicWriteKey key)
    {
        return;
    }

    public void SecondRound(Regime regime, float proportion, BudgetPool pool, LogicWriteKey key, float multiplier)
    {
        return;
    }
}