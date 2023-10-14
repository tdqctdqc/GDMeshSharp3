using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CreateWriteKey : StrongWriteKey
{
    public CreateWriteKey(Data data, ISession session) : base(data, session)
    {
    }

    public void Create<TEntity>(TEntity t) where TEntity : Entity
    {
        Data.AddEntity(t, this);
    }
    public void Create<TEntity>(IReadOnlyList<TEntity> ts) where TEntity : Entity
    {
        Data.AddEntities<TEntity>(ts, this);
    }
}