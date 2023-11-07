using Godot;
using System;

public class HostWriteKey : StrongWriteKey, ICreateWriteKey
{
    public HostLogic Logic { get; private set; }
    public HostWriteKey(HostLogic logic, ISession session) : base(session)
    {
        Logic = logic;
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
