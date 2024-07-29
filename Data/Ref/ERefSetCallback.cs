
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
    public ERefSetCallback(HashSet<ERef<TEntity>> refs) : base(refs)
    {
    }
    
    
    public void Remove(List<TEntity> ids, IWriteKey key)
    {
        ids.ForEach(id => Remove(id, key));
    }
    public void Remove(TEntity t, IWriteKey key)
    {
        base.Remove(t.MakeRef(), key);
    }
    public void Add(List<TEntity> ts, IWriteKey key)
    {
        ts.ForEach(t => Add(t, key));
    }
    public void Add(TEntity t, IWriteKey key)
    {
        base.Add(t.MakeRef(), key);
    }
}