using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class SubmitTurnCommand : Command
{
    public TurnOrders Orders { get; private set; }

    public static SubmitTurnCommand Construct(TurnOrders orders)
    {
        return new SubmitTurnCommand(orders);
    }
    [SerializationConstructor] private SubmitTurnCommand(TurnOrders orders)
    {
        Orders = orders;
    }

    public override void Enact(HostWriteKey key, Action<Procedure> queueProcedure)
    {
        var player = key.Data.BaseDomain.PlayerAux.ByGuid[CommandingPlayerGuid];
        key.Logic.SubmitPlayerTurnOrders(player, Orders);
    }

    public override bool Valid(Data data)
    {
        return true;
    }
}
