
using System.Collections.Generic;
using MessagePack;

public class MilitaryMinTurnOrders
{
    public Dictionary<int, UnitOrder> NewOrdersByGroupId { get; private set; }

    public static MilitaryMinTurnOrders Construct()
    {
        return new MilitaryMinTurnOrders(new Dictionary<int, UnitOrder>());
    }

    [SerializationConstructor]
    private MilitaryMinTurnOrders(Dictionary<int, UnitOrder> newOrdersByGroupId)
    {
        NewOrdersByGroupId = newOrdersByGroupId;
    }
}