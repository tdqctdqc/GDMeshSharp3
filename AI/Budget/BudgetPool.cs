
public class BudgetPool
{
    public IdCount<Item> OrigItems { get; private set; }
    public IdCount<Item> AvailItems { get; private set; }
    public IdCount<IModel> OrigModels { get; private set; }
    public IdCount<IModel> AvailModels { get; private set; }
    public float OrigLabor { get; set; }
    public float AvailLabor { get; set; }
    public BudgetPool(IdCount<Item> items, IdCount<IModel> flows, float origLabor)
    {
        OrigItems = IdCount<Item>.Construct(items);
        AvailItems = IdCount<Item>.Construct(items);
        OrigModels = IdCount<IModel>.Construct(flows);
        AvailModels = IdCount<IModel>.Construct(flows);
        OrigLabor = origLabor;
        AvailLabor = origLabor;
    }
}