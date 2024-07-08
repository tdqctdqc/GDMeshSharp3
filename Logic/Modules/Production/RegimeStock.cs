
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RegimeStock
{
    public IdCount<IModel> Stock { get; private set; }
    public IdCount<IModel> SingleTimeCosts { get; private set; }
    public IdCount<IModel> RecurringCosts { get; private set; }
    public IdCount<IModel> Produced { get; private set; }

    public static RegimeStock Construct()
    {
        return new RegimeStock(
            IdCount<IModel>.Construct(),
            IdCount<IModel>.Construct(),
            IdCount<IModel>.Construct(),
            IdCount<IModel>.Construct()          
        );
    }
    [SerializationConstructor] private RegimeStock(
        IdCount<IModel> stock, 
        IdCount<IModel> singleTimeCosts, 
        IdCount<IModel> recurringCosts, 
        IdCount<IModel> produced)
    {
        Stock = stock;
        SingleTimeCosts = singleTimeCosts;
        RecurringCosts = recurringCosts;
        Produced = produced;
    }

    public IEnumerable<KeyValuePair<T, float>> GetStockOfType<T>(Data d)
    {
        return Stock.GetEnumModel(d)
            .Where(kvp => kvp.Key is T)
            .Select(kvp => new KeyValuePair<T, float>((T)kvp.Key, kvp.Value));
    }
}