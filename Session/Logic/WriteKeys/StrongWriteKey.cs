using Godot;
using System;

public class StrongWriteKey : WriteKey
{
    public StrongWriteKey(Data data, ISession session) : base(data, session)
    {
    }

    public void Delete<TEntity>(TEntity t) where TEntity : Entity
    {
        Data.RemoveEntity(t.Id, this);
    }
    public void Delete(int id) 
    {
        Data.RemoveEntity(id, this);
    }
}
