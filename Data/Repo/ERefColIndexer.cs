using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ERefColIndexer<TSingle, TKey>
        where TKey : Entity 
        where TSingle : Entity
{
    public TSingle this[TKey k] => _dic.ContainsKey(k) ? _dic[k] : null;
    private Func<TSingle, IEnumerable<TKey>> _get;
    private Dictionary<TKey, TSingle> _dic;
    public ERefColIndexer(Func<TSingle, IEnumerable<TKey>> get,
        Data data) 
    {
        _get = get;
        _dic = new Dictionary<TKey, TSingle>();
        data.SubscribeForCreation<TSingle>
            (n => HandleAdded((TSingle)n.Entity));
        data.SubscribeForDestruction<TSingle>
            (n => HandleRemoved((TSingle)n.Entity));
    }

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

    public void HandleColAdd(TSingle e, TKey k)
    {
        _dic[k] = e;
    }
    public void HandleColRemove(TSingle e, TKey k)
    {
        if (_dic[k] == e)
        {
            _dic.Remove(k);
        }
    }
}
