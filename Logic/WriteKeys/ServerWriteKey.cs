using Godot;
using System;

public class ServerWriteKey : StrongWriteKey
{
    public ServerWriteKey(Data data, ISession session) : base(data, session)
    {
    }
}
