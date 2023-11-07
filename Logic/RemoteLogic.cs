using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RemoteLogic : ILogic
{
    public ProcedureWriteKey PKey { get; private set; }
    private bool _inited;
    public RemoteLogic(Data data, GameSession session)
    {
        PKey = new ProcedureWriteKey(session);
        _inited = false;
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