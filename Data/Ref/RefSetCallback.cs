
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RefSetCallback<TRef> : RefSet<TRef>
    where TRef : IdRef
{
    private Action<TRef, Data> _add, _remove;

    public static RefSetCallback<TRef> Construct(IEnumerable<TRef> refs)
    {
        return new RefSetCallback<TRef>(refs.ToHashSet());
    }
    [SerializationConstructor] protected RefSetCallback(HashSet<TRef> refs) 
        : base(refs)
    {
    }

    public void AddCallbacks(Action<TRef, Data> add, Action<TRef, Data> remove)
    {
        _add += add;
        _remove += remove;
    }
    
    public void Add(List<TRef> ids, StrongWriteKey key)
    {
        ids.ForEach(id => Add(id, key));
    }
    public void Add(TRef t, StrongWriteKey key)
    {
        base.Add(t, key);
        _add(t, key.Data);
    }
    public void Remove(List<TRef> ids, StrongWriteKey key)
    {
        ids.ForEach(id => Remove(id, key));
    }
    public void Remove(TRef t, StrongWriteKey key)
    {
        base.Remove(t, key);
        _remove(t, key.Data);
    }

    public void Clear(StrongWriteKey key)
    {
        foreach (var dRef in Refs)
        {
            _remove(dRef, key.Data);
        }
        base.Clear(key);
    }
    
    
    
    
}

public static class RefSetCallbackExt
{
    public static void AddIndexerCallbacks<TOwner, TRef, TItem>
    (   this RefSetCallback<TRef> set,
        TOwner owner, 
        Func<Data, ManyToManyIndexer<TOwner, TItem>> getIndexer)
        where TOwner : Entity where TRef : IdRef<TItem>
    {
        set.AddCallbacks((r, d) => getIndexer(d).HandleIndexAdd(owner, r.Get(d)),
            (r, d) => getIndexer(d).HandleIndexRemove(owner, r.Get(d)));
    }
    
    
    public static void AddIndexerCallbacks<TOwner, TRef, TItem>
    (   this RefSetCallback<TRef> set,
        TOwner owner, 
        Func<Data, ManyToOneIndexer<TOwner, TItem>> getIndexer)
        where TOwner : Entity where TRef : IdRef<TItem>
    {
        set.AddCallbacks((r, d) => getIndexer(d).HandleSetAdd(owner, r.Get(d)),
            (r, d) => getIndexer(d).HandleSetRemove(owner, r.Get(d)));
    }
}


