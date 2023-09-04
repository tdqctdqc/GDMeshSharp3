using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public abstract class BudgetPriority
{
    public string Name { get; private set; }
    private Func<Data, Regime, float> _getWeight;
    public float Weight { get; private set; }
    public BudgetPriority(string name, Func<Data, Regime, float> getWeight)
    {
        Name = name;
        _getWeight = getWeight;
    }

    public void SetWeight(Data data, Regime regime)
    {
        Weight = _getWeight(data, regime);
    }

    public abstract void Calculate(Regime regime, Data data,
        BudgetScratch scratch,
        Dictionary<Item, float> prices,
        MajorTurnOrders orders);

    public abstract Dictionary<Item, int> GetTradeWishlist(Regime regime, Data data,
        Dictionary<Item, float> prices,
        int credit,
        int availLabor);

}
