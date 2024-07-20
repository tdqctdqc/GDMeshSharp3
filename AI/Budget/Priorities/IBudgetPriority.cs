
using System.Collections.Generic;

public interface IBudgetPriority : INamed
{
    Dictionary<IModel, float> GetWishlist(
            Regime regime,
            Data d);
    Dictionary<IModel, float> GetWishlistCosts(
        Regime regime,
        Data d);
    bool Calculate(BudgetPool pool, Regime regime,
            LogicWriteKey key,
            out Dictionary<IModel, float> modelCosts,
            out Dictionary<IModel, float> built);
}