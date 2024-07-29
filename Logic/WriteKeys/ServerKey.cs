using Godot;
using System;

public class ServerKey : Key, IWriteKey
{
    public ServerKey(ISession session) : base(session)
    {
    }
}
