using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class StartConstructionsOrders : TurnOrderModule
{
    public List<StartConstructionRequest> ConstructionsToStart { get; private set; }
    public static StartConstructionsOrders Construct()
    {
        var c = new StartConstructionsOrders(new List<StartConstructionRequest>());
        return c;
    }
    [SerializationConstructor] private StartConstructionsOrders(List<StartConstructionRequest> constructionsToStart)
    {
        ConstructionsToStart = constructionsToStart;
    }
}
