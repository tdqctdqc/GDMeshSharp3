
using System.Collections.Generic;

public interface IBudgetPriority : INamed
{
    Dictionary<IModel, float> GetWishlist(
            Regime regime,
            Data d);
    Dictionary<IModel, float> GetWishlistCosts(
        Regime regime,
        Data d);
    void Calculate(BudgetPool pool, Regime regime,
            LogicKey key,
            out Dictionary<IModel, float> modelCosts,
            out Dictionary<string, float> built);
}