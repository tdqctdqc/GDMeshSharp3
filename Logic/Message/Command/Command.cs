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
}
