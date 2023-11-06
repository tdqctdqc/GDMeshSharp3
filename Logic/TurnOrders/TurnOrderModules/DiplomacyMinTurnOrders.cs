
using System.Collections.Generic;
using MessagePack;

public class DiplomacyMinTurnOrders : TurnOrderModule
{

    public static DiplomacyMinTurnOrders Construct()
    {
        return new DiplomacyMinTurnOrders();
    }
    [SerializationConstructor] protected DiplomacyMinTurnOrders()
    {
    }
}