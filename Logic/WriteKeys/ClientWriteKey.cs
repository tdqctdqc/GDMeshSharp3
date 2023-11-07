using Godot;
using System;

public class ClientWriteKey : WriteKey
{
    public ClientWriteKey(ISession session) : base(session)
    {
    }
}
