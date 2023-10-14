using Godot;
using System;

public class HostWriteKey : StrongWriteKey, ICreateWriteKey
{
    public HostServer HostServer { get; private set; }
    public HostLogic Logic { get; private set; }
    public HostWriteKey(HostServer hostServer, HostLogic logic, Data data, ISession session) : base(data, session)
    {
        Logic = logic;
        HostServer = hostServer;
    }

    public void Create<TEntity>(TEntity t) where TEntity : Entity
    {
        Data.AddEntity(t, this);
    }

    public void Remove<TEntity>(TEntity t) where TEntity : Entity
    {
        Data.RemoveEntity(t.Id, this);
    }
}
