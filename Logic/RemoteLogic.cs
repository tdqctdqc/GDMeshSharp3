using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RemoteLogic : ILogic
{
    public bool Calculating => false;
    public ProcedureWriteKey PKey { get; private set; }
    public RemoteLogic(Data data, GameSession session)
    {
        PKey = new ProcedureWriteKey(session);
    }


    public void Process(float delta)
    {
        
    }

    public void SubmitPlayerOrders(Player player, RegimeTurnOrders orders)
    {
        var com = SubmitTurnCommand.Construct(orders, player.PlayerGuid);
        PKey.Session.Server.QueueCommandLocal(com);
    }
}