using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class EntityRefColIndexer<TSingle, TKey>
    : AuxData<TSingle> where TKey : Entity where TSingle : Entity
{
    public TSingle this[TKey k] => _dic.ContainsKey(k) ? _dic[k] : null;
    private Func<TSingle, IEnumerable<TKey>> _get;
    private Dictionary<TKey, TSingle> _dic;
    public EntityRefColIndexer(Func<TSingle, IEnumerable<TKey>> get,
        RefColMeta<TSingle, TKey> colMeta, Data data) 
        : base(data)
    {
        _get = get;
        _dic = new Dictionary<TKey, TSingle>();
        colMeta.Added.Subscribe(HandleColAdd);
        colMeta.Removed.Subscribe(HandleColRemove);
    }

    public override void HandleAdded(TSingle added)
    {
        var keys = _get(added);
        foreach (var k in keys)
        {
            _dic.Add(k, added);
        }
    }

    public override void HandleRemoved(TSingle removing)
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

    private void HandleColAdd((TSingle e, TKey k) change)
    {
        _dic[change.k] = change.e;
    }
    private void HandleColRemove((TSingle e, TKey k) change)
    {
        if (_dic[change.k] == change.e)
        {
            _dic.Remove(change.k);
        }
    }
}
