
using System.Collections.Generic;
using MessagePack;

public class MilitaryMinTurnOrders
{
    public static MilitaryMinTurnOrders Construct()
    {
        return new MilitaryMinTurnOrders();
    }

    [SerializationConstructor]
    private MilitaryMinTurnOrders()
    {
    }
}