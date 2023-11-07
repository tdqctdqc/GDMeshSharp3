using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class SubmitTurnCommand : Command
{
    public RegimeTurnOrders Orders { get; private set; }

    public static SubmitTurnCommand Construct(RegimeTurnOrders orders, Guid guid)
    {
        
        return new SubmitTurnCommand(orders, guid);
    }
    [SerializationConstructor] private SubmitTurnCommand(RegimeTurnOrders orders, Guid commandingPlayerGuid)
        : base(commandingPlayerGuid)
    {
        Orders = orders;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var player = key.Data.BaseDomain.PlayerAux.ByGuid[CommandingPlayerGuid];
        key.Session.Logic.SubmitPlayerOrders(player, Orders);
    }

    public override bool Valid(Data data)
    {
        return true;
    }
}
