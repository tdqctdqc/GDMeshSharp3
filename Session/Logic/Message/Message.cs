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

    public byte[] Serialize(Data data)
    {
        return data.Serializer.MP.Serialize(this, GetType());
    }
}
