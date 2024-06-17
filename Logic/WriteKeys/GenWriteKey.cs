using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GenWriteKey : StrongWriteKey, IHostWriteKey
{
    public GenData GenData => (GenData) Data;
    public GenWriteKey(GenData data, ISession session) : base(session)
    {
    }

    public void Create<TEntity>(TEntity t) where TEntity : Entity
    {
        Data.AddEntity(t, this);
    }

    public void SendMessage(Message m)
    {
        if (m is Procedure p)
        {
            p.Enact(new ProcedureWriteKey(Session));
        }
        else
        {
            throw new Exception();
        }
    }
}