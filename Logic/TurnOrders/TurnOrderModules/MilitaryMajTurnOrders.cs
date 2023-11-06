using System.Collections.Generic;
using MessagePack;

public class MilitaryMajTurnOrders
{
    public static MilitaryMajTurnOrders Construct()
    {
        return new MilitaryMajTurnOrders();
    }
    [SerializationConstructor] protected MilitaryMajTurnOrders()
    {
    }
}