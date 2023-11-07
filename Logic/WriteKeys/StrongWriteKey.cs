using Godot;
using System;

public class StrongWriteKey : WriteKey
{
    public StrongWriteKey(ISession session) : base(session)
    {
    }

}
