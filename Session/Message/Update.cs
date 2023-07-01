using Godot;
using System;

public abstract class Update : Message
{
    protected Update()
    {
        
    }

    public abstract void Enact(ServerWriteKey key);

    public override void HandleHost(HostLogic logic)
    {
        return;
    }
    public override void HandleRemote(RemoteLogic logic)
    {
        logic.ProcessUpdate(this);
    }
    protected override byte GetSubMarker()
    {
        return _typeManagers[typeof(Update)].GetMarkerFromMessageType(GetType());
    }
    protected override byte GetMarker()
    {
        return _markers[typeof(Update)];
    }
}
