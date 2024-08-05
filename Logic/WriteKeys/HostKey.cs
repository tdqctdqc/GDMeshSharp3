using Godot;
using System;

public class HostKey : Key, ICreateKey
{
    public HostLogic Logic { get; private set; }
    public HostKey(HostLogic logic, ISession session) : base(session)
    {
        Logic = logic;
    }

    public void SendMessage(Message m)
    {
        Logic.HandleMessage(m);
    }


    public void Remove(Entity e)
    {
        var proc = EntityDeletionUpdate.Create(e.Id, this);
        SendMessage(proc);
    }

    public void Create<TEntity>(TEntity t) where TEntity : Entity
    {
        var update = EntityCreationUpdate<TEntity>.Create(t, this);
        SendMessage(update);
    }
}
