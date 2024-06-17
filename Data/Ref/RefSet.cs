using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RefSet<TRef> where TRef : IdRef
{
    public HashSet<TRef> Refs { get; private set; }

    [SerializationConstructor] protected RefSet(HashSet<TRef> refs)
    {
        Refs = refs;
    }

    protected void Add(TRef t, StrongWriteKey key)
    {
        Refs.Add(t);
    }
    protected void Remove(TRef t, StrongWriteKey key)
    {
        Refs.Remove(t);
    }

    protected void Clear(StrongWriteKey key)
    {
        Refs.Clear();
    }

    public int Count()
    {
        return Refs.Count;
    }

    public bool Contains(TRef t)
    {
        return Refs.Contains(t);
    }
}

public static class RefSetExt
{

    public static IEnumerable<TItem> Get<TItem, TRef>(
        this RefSet<TRef> set, Data d)
        where TRef : IdRef<TItem>
    {
        return set.Refs.Select(r => r.Get(d));
    }
        
    public static bool Contains
    (this RefSet<CellRef> set,
        int id)
    {
        return set.Contains(new CellRef(id));
    }
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
        return set.Refs.Select(r => r.Get(d));
    }
}