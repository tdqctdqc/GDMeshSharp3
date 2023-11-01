using System.Collections.Generic;
using MessagePack;

public class MilitaryMajTurnOrders
{
    // public List<int> UnitTemplatesToForm { get; private set; }
    // public List<List<int>> NewGroupUnits { get; private set; }
    public static MilitaryMajTurnOrders Construct()
    {
        return new MilitaryMajTurnOrders();
    }
    [SerializationConstructor] protected MilitaryMajTurnOrders()
    {
        // UnitTemplatesToForm = unitTemplatesToForm;
        // NewGroupUnits = newGroupUnits;
    }
}