using System.Collections.Generic;
using MessagePack;

public class MilitaryOrders
{
    public List<int> UnitTemplatesToForm { get; private set; }

    public static MilitaryOrders Construct()
    {
        return new MilitaryOrders();
    }
    [SerializationConstructor] protected MilitaryOrders()
    {
        UnitTemplatesToForm = new List<int>();
    }
}