using Godot;
using System;
using MessagePack;

public abstract class Command : Message
{
    public Guid CommandingPlayerGuid { get; private set; }
    protected Command(Guid commandingPlayerGuid)
    {
        CommandingPlayerGuid = commandingPlayerGuid;
    }
    public abstract bool Valid(Data data);

    public override void Enact(ProcedureWriteKey key)
    {
        key.Data.Requests.QueueCommand.Invoke(this);
    }
}
