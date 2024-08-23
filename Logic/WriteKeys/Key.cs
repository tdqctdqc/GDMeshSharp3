using Godot;
using System;

public class Key : IWriteKey
{
    public Data Data => Session.Data;
    public Data GetData() => Data;
    public ISession Session { get; private set; }
    public Key(ISession session)
    {
        Session = session;
    }
}
