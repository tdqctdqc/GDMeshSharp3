
public class BudgetPool
{
    public IdCount<Item> OrigItems { get; private set; }
    public IdCount<Item> AvailItems { get; private set; }
    public IdCount<Flow> OrigFlows { get; private set; }
    public IdCount<Flow> AvailFlows { get; private set; }
    public float OrigLabor { get; set; }
    public float AvailLabor { get; set; }
    public BudgetPool(IdCount<Item> items, IdCount<Flow> flows, float origLabor)
    {
        OrigItems = IdCount<Item>.Construct(items);
        AvailItems = IdCount<Item>.Construct(items);
        OrigFlows = IdCount<Flow>.Construct(flows);
        AvailFlows = IdCount<Flow>.Construct(flows);
        OrigLabor = origLabor;
        AvailLabor = origLabor;
    }
}