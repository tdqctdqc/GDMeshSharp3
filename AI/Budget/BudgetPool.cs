
public class BudgetPool
{
    public ItemCount OrigItems { get; private set; }
    public ItemCount AvailItems { get; private set; }
    public FlowCount OrigFlows { get; private set; }
    public FlowCount AvailFlows { get; private set; }
    public float OrigLabor { get; set; }
    public float AvailLabor { get; set; }
    public BudgetPool(ItemCount items, FlowCount flows, float origLabor)
    {
        OrigItems = ItemCount.Construct(items);
        AvailItems = ItemCount.Construct(items);
        OrigFlows = FlowCount.Construct(flows);
        AvailFlows = FlowCount.Construct(flows);
        OrigLabor = origLabor;
        AvailLabor = origLabor;
    }
}