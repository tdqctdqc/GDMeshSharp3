using Godot;
using System;
using System.Collections.Generic;
using MessagePack;

public struct ERef<TRef> where TRef : Entity
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
    public TRef Entity(Data data)
    {
        if (RefId == -1) return null;
        return data.Get<TRef>(RefId);
    }

    public bool IsEmpty()
    {
        return RefId == -1;
    }

    public bool Fulfilled()
    {
        return RefId != -1;
    }
}

public static class ERefExt
{
    public static bool Contains<TEntity>(this HashSet<ERef<TEntity>> set, int id)
        where TEntity : Entity
    {
        return set.Contains(new ERef<TEntity>(id));
    }

    public static void Remove<TEntity>(this HashSet<ERef<TEntity>> set, int id)
        where TEntity : Entity
    {
        set.Remove(new ERef<TEntity>(id));
    }
    public static void Add<TEntity>(this HashSet<ERef<TEntity>> set, int id)
        where TEntity : Entity
    {
        set.Add(new ERef<TEntity>(id));
    }
}
