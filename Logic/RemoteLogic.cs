using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RemoteLogic : ILogic
{
    public ProcedureWriteKey PKey { get; private set; }
    private bool _inited;
    private List<Update> _syncingUpdates;
    public RemoteLogic(Data data, GameSession session)
    {
        PKey = new ProcedureWriteKey(data, session);
        _inited = false;
        _syncingUpdates = new List<Update>();
    }

    public void Process(float delta)
    {
        
    }
    public void ProcessUpdate(Update u)
    {
        if(_inited)
        {
            u.Enact(PKey);
            return;
        }
        if (u is FinishedStateSyncUpdate su)
        {
            _inited = true;
            var creations = _syncingUpdates.SelectWhereOfType<EntityCreationUpdate>();
            EntitiesCreationUpdate.Create(creations, PKey).Enact(PKey);
            su.Enact(PKey);
            return;
        }
        _syncingUpdates.Add(u);
    }
}