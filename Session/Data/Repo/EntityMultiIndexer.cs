
using System;
using System.Collections.Generic;
using System.Linq;

public class EntityMultiIndexer<TSingle, TMult> : AuxData<TMult>
    where TSingle : Entity where TMult : Entity
{
    public List<TMult> Get(TSingle s, Data data)
    {
        return _dic.ContainsKey(s.Id) 
            ? _dic[s.Id].Select(i => data.RefFulfiller.Get<TMult>(i)).ToList() 
            : null;
    }
    protected Dictionary<int, HashSet<int>> _dic;
    private Func<TMult, EntityRef<TSingle>> _getSingle;
    private RefAction<ValChangeNotice<EntityRef<TSingle>>> _changedMult;
    public EntityMultiIndexer(Data data, Func<TMult, EntityRef<TSingle>> getSingle,
        RefAction[] recalcTriggers,
        RefAction<ValChangeNotice<EntityRef<TSingle>>>[] changeTriggers) : base(data)
    {
        _dic = new Dictionary<int, HashSet<int>>();
        _getSingle = getSingle;
        _changedMult = new RefAction<ValChangeNotice<EntityRef<TSingle>>>();
        _changedMult.Subscribe(n => 
        {
            if (_dic.TryGetValue(n.OldVal.RefId, out var hash))
            {
                hash.Remove(n.Entity.Id);
            }
            _dic.AddOrUpdate(n.NewVal.RefId, n.Entity.Id);
        });
        foreach (var recalcTrigger in recalcTriggers)
        {
            recalcTrigger.Subscribe(() => Recalc(data));
        }
        foreach (var changeTrigger in changeTriggers)
        {
            changeTrigger.Subscribe(_changedMult);
        }
        data.SubscribeForDestruction<TSingle>(HandleTSingleRemoved);
    }

    private void Recalc(Data data)
    {
        _dic.Clear();
        var mults = data.GetRegister<TMult>().Entities;
        foreach (var entity in mults)
        {
            HandleAdded(entity);
        }
    }
    public override void HandleAdded(TMult added)
    {
        var single = _getSingle(added);
        if(single != null) _dic.AddOrUpdate(single.RefId, added.Id);
    }

    private void HandleTSingleRemoved(EntityDestroyedNotice n)
    {
        _dic.Remove(n.Entity.Id);
    }
    public override void HandleRemoved(TMult removing)
    {
        var single = _getSingle(removing);
        if(single != null) _dic[single.RefId].Remove(removing.Id);
    }
}
