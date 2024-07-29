
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
    
    public void Add(List<TRef> ids, IWriteKey key)
    {
        ids.ForEach(id => Add(id, key));
    }
    public override void Add(TRef t, IWriteKey key)
    {
        Refs.Add(t);
        _add(t, key.GetData());
    }
    public void Remove(List<TRef> ids, IWriteKey key)
    {
        ids.ForEach(id => Remove(id, key));
    }
    public override void Remove(TRef t, IWriteKey key)
    {
        Refs.Remove(t);
        _remove(t, key.GetData());
    }

    public override void Clear(IWriteKey key)
    {
        foreach (var dRef in Refs)
        {
            _remove(dRef, key.GetData());
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


