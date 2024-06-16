using Godot;
using System;
using System.Collections.Generic;
using MessagePack;

public struct ERef<TRef> : IdRef
    where TRef : Entity
{
    public int RefId { get; }

    public ERef()
    {
        throw new Exception();
    }
    public ERef(TRef entity)
    {
        RefId = entity.Id;
    }

    public static ERef<TRef> GetEmpty()
    {
        return new ERef<TRef>(-1);
    }
    [SerializationConstructor] public ERef(int refId)
    {
        RefId = refId;
    }

    IIdentifiable IdRef.Get(Data data) => Get(data);
    public TRef Get(Data data)
    {
        if (RefId == -1) return null;
        return data.Get<TRef>(RefId);
    }

    
}
