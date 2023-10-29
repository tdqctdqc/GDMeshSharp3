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
        PKey = new ProcedureWriteKey(data);
        _inited = false;
    }

    public void Process(float delta)
    {
        
    }
}