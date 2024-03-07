using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GenWriteKey : StrongWriteKey, ICreateWriteKey
{
    public GenData GenData => (GenData) Data;
    public GenWriteKey(GenData data, ISession session) : base(session)
    {
    }

    public void Create<TEntity>(TEntity t) where TEntity : Entity
    {
        Data.AddEntity(t, this);
    }

}