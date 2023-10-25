
using System.Collections.Generic;
using MessagePack;

public class MilitaryMinTurnOrders
{
    public Dictionary<int, UnitOrder> NewOrdersByGroupId { get; private set; }
    public Dictionary<int, int> NewGroupAssignmentsByUnitId { get; private set; }
    public static MilitaryMinTurnOrders Construct()
    {
        return new MilitaryMinTurnOrders(new Dictionary<int, UnitOrder>(),
            new Dictionary<int, int>());
    }

    [SerializationConstructor]
    private MilitaryMinTurnOrders(
        Dictionary<int, UnitOrder> newOrdersByGroupId,
        Dictionary<int, int> newGroupAssignmentsByUnitId)
    {
        NewOrdersByGroupId = newOrdersByGroupId;
        NewGroupAssignmentsByUnitId = newGroupAssignmentsByUnitId;
    }
}