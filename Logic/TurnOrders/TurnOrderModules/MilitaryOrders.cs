using System.Collections.Generic;
using MessagePack;

public class MilitaryOrders
{
    public List<int> UnitTemplatesToBuild { get; private set; }

    public static MilitaryOrders Construct()
    {
        return new MilitaryOrders();
    }
    [SerializationConstructor] protected MilitaryOrders()
    {
        UnitTemplatesToBuild = new List<int>();
    }
}