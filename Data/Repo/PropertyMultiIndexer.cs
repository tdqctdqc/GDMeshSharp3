
using System;
using System.Collections.Generic;

public class PropertyMultiIndexer<TSingle, TMult>
    where TMult : Entity
{
    public HashSet<TMult> this[TSingle t] => _dic.ContainsKey(t) ? _dic[t] : null;
    protected Dictionary<TSingle, HashSet<TMult>> _dic;
    private Func<TMult, TSingle> _getSingle;
    public PropertyMultiIndexer(Data data, Func<TMult, TSingle> getSingle,
        RefAction[] recalcTriggers,
        params ValChangeAction<TMult, TSingle>[] changeTriggers)
    {
        _dic = new Dictionary<TSingle, HashSet<TMult>>();
        _getSingle = getSingle;
        foreach (var recalcTrigger in recalcTriggers)
        {
            recalcTrigger.Subscribe(() => Recalc(data));
        }
        foreach (var changeTrigger in changeTriggers)
        {
            changeTrigger.Subscribe(HandleValChanged);
        }
    }

    private void HandleValChanged(ValChangeNotice<TMult, TSingle> n)
    {
        if (n.OldVal != null && _dic.TryGetValue(n.OldVal, out var hash))
        {
            hash.Remove((TMult)n.Entity);
        }
        _dic.AddOrUpdate(n.NewVal, (TMult)n.Entity);
    }
    private void Recalc(Data data)
    {
        _dic.Clear();
        var mults = data.GetAll<TMult>();
        foreach (var entity in mults)
        {
            HandleAdded(entity);
        }
    }
    public void HandleAdded(TMult added)
    {
        var single = _getSingle(added);
        if(single != null) _dic.AddOrUpdate(single, added);
    }

    public void HandleRemoved(TMult removing)
    {
        var single = _getSingle(removing);
        if(single != null) _dic[single].Remove(removing);
    }
}