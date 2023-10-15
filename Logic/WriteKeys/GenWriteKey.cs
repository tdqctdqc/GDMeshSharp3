using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GenWriteKey : StrongWriteKey, ICreateWriteKey
{
    public GenData GenData => (GenData) Data;
    public GenWriteKey(GenData data) : base(data)
    {
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