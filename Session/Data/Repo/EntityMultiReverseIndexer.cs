using System;
using System.Collections.Generic;
using System.Linq;

public class EntityMultiReverseIndexer<TSingle, TKey>
    : AuxData<TSingle> where TKey : Entity where TSingle : Entity
{
    public TSingle this[TKey k] => _dic.ContainsKey(k) ? _dic[k] : null;
    private Func<TSingle, IEnumerable<TKey>> _get;
    private Dictionary<TKey, TSingle> _dic;
    public EntityMultiReverseIndexer(Func<TSingle, IEnumerable<TKey>> get,
        RefAction<(TKey, TSingle)> change, Data data) 
        : base(data)
    {
        _get = get;
        _dic = new Dictionary<TKey, TSingle>();
        change.Subscribe(HandleChange);
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
            _dic.Remove(k);
        }
    }

    private void HandleChange((TKey, TSingle) change)
    {
        _dic[change.Item1] = change.Item2;
    }
}
