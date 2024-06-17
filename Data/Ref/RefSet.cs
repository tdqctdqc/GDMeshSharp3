using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RefSet<TRef> where TRef : IdRef
{
    public HashSet<TRef> Items { get; private set; }

    [SerializationConstructor] protected RefSet(HashSet<TRef> items)
    {
        Items = items;
    }

    protected void Add(TRef t, StrongWriteKey key)
    {
        Items.Add(t);
    }
    protected void Remove(TRef t, StrongWriteKey key)
    {
        Items.Remove(t);
    }

    public int Count()
    {
        return Items.Count;
    }

    public bool Contains(TRef t)
    {
        return Items.Contains(t);
    }
}

public static class RefSetExt
{
    public static bool Contains<TEntity>
        (this RefSet<ERef<TEntity>> set,
            TEntity t)
                where TEntity : Entity
    {
        return set.Contains(t.MakeRef());
    }
    public static bool Contains<TEntity>
        (this RefSet<ERef<TEntity>> set,
            int id)
            where TEntity : Entity
    {
        return set.Contains(new ERef<TEntity>(id));
    }
    
    public static IEnumerable<TEntity> Entities<TEntity>(
        this RefSet<ERef<TEntity>> set,
        Data d)
            where TEntity : Entity
    {
        return set.Items.Select(r => r.Get(d));
    }
}