using Godot;
using System;

public class ClientWriteKey : WriteKey
{
    public ClientWriteKey(Data data, ISession session) : base(data, session)
    {
    }
}
