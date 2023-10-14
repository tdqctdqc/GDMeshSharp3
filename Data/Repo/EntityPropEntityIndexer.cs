
using System;

public class EntityPropEntityIndexer<TEntity, TKey> : PropEntityIndexer<TEntity, int?>
    where TEntity : Entity where TKey : Entity
{
    public TEntity this[TKey k] => this[k.Id];
    public static EntityPropEntityIndexer<TEntity, TKey> CreateStatic(Data data, Func<TEntity, EntityRef<TKey>> get)
    {
        return new EntityPropEntityIndexer<TEntity, TKey>(data, get);
    }
    public static EntityPropEntityIndexer<TEntity, TKey> CreateDynamic(Data data, Func<TEntity, EntityRef<TKey>> get,
        params ValChangeAction<TEntity, EntityRef<TKey>>[] changedValTriggers)
    {
        return new EntityPropEntityIndexer<TEntity, TKey>(data, get, changedValTriggers);
    }
    private EntityPropEntityIndexer(Data data, Func<TEntity, EntityRef<TKey>> get,
        params ValChangeAction<TEntity, EntityRef<TKey>>[] changedValTriggers) 
        : base(data, e => get(e) == null ? (int?)null : get(e).RefId)
    {
        foreach (var trigger in changedValTriggers)
        {
            trigger.Subscribe(n =>
            {
                var t = (TEntity) n.Entity;
                if (n.OldVal != null)
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
