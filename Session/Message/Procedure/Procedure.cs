using Godot;
using System;

public abstract class Procedure : Message
{
    protected Procedure()
    {
        
    }
    
    public override void HandleHost(HostLogic logic)
    {
        return;
    }
    public override void HandleRemote(RemoteLogic logic)
    {
        logic.ProcessProcedure(this);
    }
    protected override byte GetSubMarker()
    {
        return _typeManagers[typeof(Procedure)].GetMarkerFromMessageType(GetType());
    }
    protected override byte GetMarker()
    {
        return _markers[typeof(Procedure)];
    }
    public abstract bool Valid(Data data);
    public abstract void Enact(ProcedureWriteKey key);
}

