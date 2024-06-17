
using System;
using System.Collections.Generic;

public class ERefSetCallback<TEntity> : RefSetCallback<ERef<TEntity>>
    where TEntity : Entity
{
    public static ERefSetCallback<TEntity> Construct(IEnumerable<ERef<TEntity>> items)
    {
        var col = new ERefSetCallback<TEntity>(items.ToHashSet());
        return col;
    }
    public ERefSetCallback(HashSet<ERef<TEntity>> items) : base(items)
    {
    }
    
    public void SetIndexerCallbacks<TOwner>
    (TOwner owner, 
        Func<Data, ERefColIndexer<TOwner, TEntity>> getIndexer)
        where TOwner : Entity 
    {
        _add = (r, d) => getIndexer(d).HandleColAdd(owner, r.Get(d));
        _remove = (r, d) => getIndexer(d).HandleColRemove(owner, r.Get(d));
    }
    public void Remove(List<TEntity> ids, StrongWriteKey key)
    {
        ids.ForEach(id => Remove(id, key));
    }
    public void Remove(TEntity t, StrongWriteKey key)
    {
        Remove(t.MakeRef(), key);
        _remove(t.MakeRef(), key.Data);
    }
    
    
    public void Add(List<TEntity> ts, StrongWriteKey key)
    {
        ts.ForEach(t => Add(t, key));
    }
    public void Add(TEntity t, StrongWriteKey key)
    {
        Add(t.MakeRef(), key);
        _add(t.MakeRef(), key.Data);
    }
}