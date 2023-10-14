using Godot;
using System;

public class HostWriteKey : CreateWriteKey
{
    public HostServer HostServer { get; private set; }
    public HostLogic Logic { get; private set; }
    public HostWriteKey(HostServer hostServer, HostLogic logic, Data data, ISession session) : base(data, session)
    {
        Logic = logic;
        HostServer = hostServer;
    }
}
