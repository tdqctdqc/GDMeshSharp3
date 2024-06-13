
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ERefSetCallback<TRef> : RefSet<ERef<TRef>>
    where TRef : Entity
{
    private Action<TRef, Data> _add, _remove;
    public static ERefSetCallback<TRef> Construct(IEnumerable<ERef<TRef>> items)
    {
        var col = new ERefSetCallback<TRef>(items.ToHashSet());
        return col;
    }
    public static ERefSetCallback<TRef> Construct(HashSet<int> items)
    {
        var col = new ERefSetCallback<TRef>(items.Select(id => new ERef<TRef>(id)).ToHashSet());
        return col;
    }
    [SerializationConstructor] protected ERefSetCallback(HashSet<ERef<TRef>> items) 
        : base(items)
    {
    }

    public void SetCallbacks(Action<TRef, Data> add, Action<TRef, Data> remove)
    {
        _add = add;
        _remove = remove;
    }

    public void SetIndexerCallbacks<TOwner>
        (TOwner owner, Func<Data, ERefColIndexer<TOwner, TRef>> getIndexer)
            where TOwner : Entity
    {
        _add = (r, d) => getIndexer(d).HandleColAdd(owner, r);
        _remove = (r, d) => getIndexer(d).HandleColRemove(owner, r);
    }
    public IEnumerable<TRef> Entities(Data d)
    {
        return Items.Select(r => r.Get(d));
    }
    public void Add(List<TRef> ids, StrongWriteKey key)
    {
        ids.ForEach(id => Add(id, key));
    }
    public void Add(TRef t, StrongWriteKey key)
    {
        Add(t.MakeRef(), key);
        _add(t, key.Data);
    }
    public void Remove(List<TRef> ids, StrongWriteKey key)
    {
        ids.ForEach(id => Remove(id, key));
    }
    public void Remove(TRef t, StrongWriteKey key)
    {
        Remove(t.MakeRef(), key);
        _remove(t, key.Data);
    }
    public bool Contains(int id)
    {
        return Contains(new ERef<TRef>(id));
    }
    public bool Contains(TRef t)
    {
        return Contains(t.MakeRef());
    }
}