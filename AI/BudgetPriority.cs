using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public abstract class BudgetPriority
{
    private Func<Data, Regime, float> _getWeight;
    public float Weight { get; private set; }
    public BudgetPriority(Func<Data, Regime, float> getWeight)
    {
        _getWeight = getWeight;
    }

    public void SetWeight(Data data, Regime regime)
    {
        Weight = _getWeight(data, regime);
    }

    public abstract void Calculate(Regime regime, Data data,
        ItemWallet budget,
        Dictionary<Item, float> prices,
        int credit,
        int availLabor,
        MajorTurnOrders orders);

    public abstract Dictionary<Item, int> GetItemWishlist(Regime regime, Data data,
        Dictionary<Item, float> prices,
        int credit,
        int availLabor);

}
