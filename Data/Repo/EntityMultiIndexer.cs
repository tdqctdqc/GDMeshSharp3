
using System;
using System.Collections.Generic;
using System.Linq;

public class EntityMultiIndexer<TSingle, TMult> : AuxData<TMult>
    where TSingle : Entity where TMult : Entity
{
    public HashSet<TMult> this[TSingle t] => _dic.ContainsKey(t) ? _dic[t] : null;
    protected Dictionary<TSingle, HashSet<TMult>> _dic;
    private Func<TMult, TSingle> _getSingle;
    public EntityMultiIndexer(Data data, Func<TMult, TSingle> getSingle,
        RefAction[] recalcTriggers,
        params ValChangeAction<TMult, TSingle>[] changeTriggers) : base(data)
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
        data.SubscribeForDestruction<TSingle>(HandleTSingleRemoved);
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
    public override void HandleAdded(TMult added)
    {
        var single = _getSingle(added);
        if(single != null) _dic.AddOrUpdate(single, added);
    }

    private void HandleTSingleRemoved(EntityDestroyedNotice n)
    {
        _dic.Remove((TSingle)n.Entity);
    }
    public override void HandleRemoved(TMult removing)
    {
        var single = _getSingle(removing);
        if(single != null) _dic[single].Remove(removing);
    }
}
