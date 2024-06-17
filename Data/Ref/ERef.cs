using Godot;
using System;
using System.Collections.Generic;
using MessagePack;

public struct ERef<TEntity> : IdRef<TEntity>
    where TEntity : Entity
{
    public int RefId { get; }

    public ERef()
    {
        throw new Exception();
    }
    public ERef(TEntity entity)
    {
        RefId = entity.Id;
    }

    public static ERef<TEntity> GetEmpty()
    {
        return new ERef<TEntity>(-1);
    }
    [SerializationConstructor] public ERef(int refId)
    {
        RefId = refId;
    }

    IIdentifiable IdRef.Get(Data data) => Get(data);
    public TEntity Get(Data data)
    {
        if (RefId == -1) return null;
        return data.Get<TEntity>(RefId);
    }

    
}
