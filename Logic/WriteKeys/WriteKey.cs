using Godot;
using System;

public class WriteKey
{
    public Data Data => Session.Data;
    public ISession Session { get; private set; }
    public WriteKey(ISession session)
    {
        Session = session;
    }
}
