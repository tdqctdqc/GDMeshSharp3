using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public abstract class Message
{
    // public abstract void HandleHost(HostLogic logic);
    // public abstract void HandleRemote(RemoteLogic logic);
    public abstract void Enact(ProcedureWriteKey key);

    public MessageWrapper Wrap()
    {
        return new MessageWrapper(this);
    }

    public byte[] Serialize()
    {
        return Game.I.Serializer.MP.Serialize(this, GetType());
    }
}
