using System.Collections.Generic;
using MessagePack;

public class MilitaryMajTurnOrders
{
    public List<int> UnitTemplatesToForm { get; private set; }
    public List<List<int>> NewGroupUnits { get; private set; }
    public static MilitaryMajTurnOrders Construct()
    {
        return new MilitaryMajTurnOrders(new List<int>(),
            new List<List<int>>());
    }
    [SerializationConstructor] protected MilitaryMajTurnOrders(List<int> unitTemplatesToForm,
        List<List<int>> newGroupUnits)
    {
        UnitTemplatesToForm = unitTemplatesToForm;
        NewGroupUnits = newGroupUnits;
    }
}