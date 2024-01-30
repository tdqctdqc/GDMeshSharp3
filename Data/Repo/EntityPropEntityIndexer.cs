
using System;

public class EntityPropEntityIndexer<TEntity, TKey> 
    : PropEntityIndexer<TEntity, int?>
    where TEntity : Entity where TKey : Entity
{
    public TEntity this[TKey k] => this[k.Id];
    public static EntityPropEntityIndexer<TEntity, TKey> CreateStatic(Data data, Func<TEntity, ERef<TKey>> get)
    {
        return new EntityPropEntityIndexer<TEntity, TKey>(data, get);
    }
    private EntityPropEntityIndexer(Data data, Func<TEntity, ERef<TKey>> get,
        params ValChangeAction<TEntity, ERef<TKey>>[] changedValTriggers) 
        : base(data, e => get(e).IsEmpty() ? (int?)null : get(e).RefId)
    {
        foreach (var trigger in changedValTriggers)
        {
            trigger.Subscribe(n =>
            {
                var t = (TEntity) n.Entity;
                if (n.OldVal.Fulfilled())
                {
                    if (_dic.ContainsKey(n.OldVal.RefId) == false) throw new Exception();
                    if (_dic[n.OldVal.RefId] == t)
                    {
                        _dic.Remove(n.OldVal.RefId);
                    }
                }
                _dic.Add(n.NewVal.RefId, (TEntity)n.Entity);
            });
        }
    }

    public bool ContainsKey(TKey k)
    {
        return ContainsKey(k.Id);
    }
}
