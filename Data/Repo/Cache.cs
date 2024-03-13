using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class Cache
{
    public static Cache<TEntity, TValue> MakeForEntity<TEntity, TValue>
        (Func<TEntity, TValue> get, Data d)
            where TEntity : Entity
    {
        var c = new Cache<TEntity, TValue>(get, () => d.GetAll<TEntity>());
        d.SubscribeForCreation<TEntity>(n => c.HandleAdded((TEntity)n.Entity));
        d.SubscribeForDestruction<TEntity>(n => c.HandleRemoved((TEntity)n.Entity));
        return c;
    }
}


public class Cache<TOwner, TValue> 
{
    public TValue this[TOwner t] => _dic.ContainsKey(t) ? _dic[t] : default;
    public IReadOnlyDictionary<TOwner, TValue> Dic => _dic;
    protected Dictionary<TOwner, TValue> _dic;
    protected Func<TOwner, TValue> _get;
    protected Func<IEnumerable<TOwner>> _getAll;
    

    public Cache(Func<TOwner, TValue> get,
        Func<IEnumerable<TOwner>> getAll)
    {
        _getAll = getAll;
        _dic = new Dictionary<TOwner, TValue>();
        _get = get;
        foreach (var entity in _getAll())
        {
            HandleAdded(entity);
        }
    }

    private void HandleUpdated(TOwner e, TValue newVal)
    {
        _dic[e] = newVal;
    }
    public void HandleAdded(TOwner added)
    {
        _dic.Add(added, _get(added)); ;
    }

    public void HandleRemoved(TOwner removing)
    {
        _dic.Remove(removing);
    }
}
