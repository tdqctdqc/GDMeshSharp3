using System.Linq;
public class BudgetPool
{
    public IdCount<IModel> AvailModels { get; private set; }

    public static BudgetPool ConstructForRegime(Regime r, Data d)
    {
        var models = IdCount<IModel>.Construct();
        models.Add(r.Stock.Stock);
        return new BudgetPool(models);
    }
    
    public BudgetPool(IdCount<IModel> models)
    {
        AvailModels = IdCount<IModel>.Construct(models);
    }
}