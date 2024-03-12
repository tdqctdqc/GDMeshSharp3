
using System.Collections.Generic;

public interface IBudgetPriority
{
    string Name { get; }

    Dictionary<IModel, float> GetWishlistCosts(
            Regime regime,
            Data d);
    bool Calculate(BudgetPool pool, Regime regime,
            LogicWriteKey key,
            out Dictionary<IModel, float> modelCosts);
}