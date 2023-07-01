using Godot;
using System;
using MessagePack;

public abstract class Command : Message
{
    [IgnoreMember] public Guid CommandingPlayerGuid { get; private set; }
    protected Command()
    {
        
    }
    public abstract void Enact(HostWriteKey key, Action<Procedure> queueProcedure);
    public abstract bool Valid(Data data);

    public void SetGuid(Guid guid)
    {
        CommandingPlayerGuid = guid;
    }
    public override void HandleHost(HostLogic logic)
    {
        logic.CommandQueue.Enqueue(this);
    }
    public override void HandleRemote(RemoteLogic logic)
    {
        return;
    }
    protected override byte GetSubMarker()
    {
        return _typeManagers[typeof(Command)].GetMarkerFromMessageType(GetType());
    }
    protected override byte GetMarker()
    {
        return _markers[typeof(Command)];
    }
}
