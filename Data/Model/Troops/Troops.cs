
public class Troops : ModelList<Troop>
{
    public Troop Rifle1 { get; private set; }
    public Troops(Items items, FlowList flows)
    {
        Rifle1 = new Rifle1(items, flows);
    }
}