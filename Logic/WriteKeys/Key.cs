using Godot;
using System;

public abstract class Key : IWriteKey
{
    public Data Data => Session.Data;
    public Data GetData() => Data;
    public abstract bool HasRemotes();
    public ISession Session { get; private set; }
    public Key(ISession session)
    {
        Session = session;
    }
}
