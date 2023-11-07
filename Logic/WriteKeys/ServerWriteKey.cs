using Godot;
using System;

public class ServerWriteKey : StrongWriteKey
{
    public ServerWriteKey(ISession session) : base(session)
    {
    }
}
