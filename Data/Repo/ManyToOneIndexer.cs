using System;
using System.Collections.Generic;
using System.Linq;
using Godot;



public class ManyToOneIndexer
{
    public static ManyToOneIndexer<TSingle, TKey>
        MakeForEntity<TSingle, TKey>(
            Func<TSingle, RefSet<ERef<TKey>>> get,
            Data data)
            where TSingle : Entity where TKey : Entity
    {
        var indexer = new ManyToOneIndexer<TSingle, TKey>(
            s => get(s).Get<TKey, ERef<TKey>>(data));
        
        data.SubscribeForCreation<TSingle>
            (n => indexer.HandleAdded((TSingle)n.Entity));
        data.SubscribeForDestruction<TSingle>
            (n => indexer.HandleRemoved((TSingle)n.Entity));
        return indexer;
    }
    
    public static ManyToOneIndexer<TSingle, TKey>
        MakeForEntity<TSingle, TKey>(
            Func<TSingle, IEnumerable<TKey>> get,
            Data data)
        where TSingle : Entity where TKey : Entity
    {
        var indexer = new ManyToOneIndexer<TSingle, TKey>(
            get);
        
        data.SubscribeForCreation<TSingle>
            (n => indexer.HandleAdded((TSingle)n.Entity));
        data.SubscribeForDestruction<TSingle>
            (n => indexer.HandleRemoved((TSingle)n.Entity));
        return indexer;
    }
}
public class ManyToOneIndexer<TSingle, TKey>
    where TSingle : class
{
    public TSingle this[TKey k] => _dic.ContainsKey(k) ? _dic[k] : null;
    private Func<TSingle, IEnumerable<TKey>> _get;
    private Dictionary<TKey, TSingle> _dic;
    public ManyToOneIndexer(Func<TSingle, IEnumerable<TKey>> get) 
    {
        _get = get;
        _dic = new Dictionary<TKey, TSingle>();
    }

    public IEnumerable<TKey> Keys() => _dic.Keys;
    public IEnumerable<TSingle> Values() => _dic.Values.Distinct();

    public void HandleAdded(TSingle added)
    {
        var keys = _get(added);
        foreach (var k in keys)
        {
            _dic.Add(k, added);
        }
    }

    public void HandleRemoved(TSingle removing)
    {
        var keys = _get(removing);
        foreach (var k in keys)
        {
            if (_dic[k] == removing)
            {
                _dic.Remove(k);
            }
        }
    }

    public void HandleSetAdd(TSingle e, TKey k)
    {
        _dic[k] = e;
    }
    public void HandleSetRemove(TSingle e, TKey k)
    {
        if (_dic[k] == e)
        {
            _dic.Remove(k);
        }
    }
}
