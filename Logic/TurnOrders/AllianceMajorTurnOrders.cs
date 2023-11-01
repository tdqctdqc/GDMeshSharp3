
using System.Collections.Generic;
using MessagePack;

public class AllianceMajorTurnOrders
{
    public static AllianceMajorTurnOrders Construct()
    {
        return new AllianceMajorTurnOrders();
    }

    [SerializationConstructor] private AllianceMajorTurnOrders()
    {
    }
}