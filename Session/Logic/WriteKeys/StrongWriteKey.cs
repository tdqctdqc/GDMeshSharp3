using Godot;
using System;

public class StrongWriteKey : WriteKey
{
    public StrongWriteKey(Data data, ISession session) : base(data, session)
    {
    }

}
