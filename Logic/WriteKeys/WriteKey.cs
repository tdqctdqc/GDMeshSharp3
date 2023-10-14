using Godot;
using System;

public class WriteKey
{
    public Data Data { get; private set; }
    public ISession Session { get; private set; }
    
    public WriteKey(Data data, ISession session)
    {
        Session = session;
        Data = data;
    }
}
