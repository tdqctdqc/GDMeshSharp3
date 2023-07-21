using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class SubmitTurnCommand : Command
{
    public TurnOrders Orders { get; private set; }

    public static SubmitTurnCommand Construct(TurnOrders orders, Guid guid)
    {
        
        return new SubmitTurnCommand(orders, guid);
    }
    [SerializationConstructor] private SubmitTurnCommand(TurnOrders orders, Guid commandingPlayerGuid)
        : base(commandingPlayerGuid)
    {
        Orders = orders;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var player = key.Data.BaseDomain.PlayerAux.ByGuid[CommandingPlayerGuid];
        key.Data.Requests.SubmitPlayerOrders.Invoke((player, Orders));
    }

    public override bool Valid(Data data)
    {
        return true;
    }
}
