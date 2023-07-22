
using System;
using System.Collections.Generic;
using System.Linq;

public class EntityMultiIndexer<TMulti, TMult> : AuxData<TMult>
    where TMulti : Entity where TMult : Entity
{
    public List<TMult> Get(TMulti s, Data data)
    {
        return _dic.ContainsKey(s.Id) 
            ? _dic[s.Id].Select(i => data.RefFulfiller.Get<TMult>(i)).ToList() 
            : null;
    }
    protected Dictionary<int, HashSet<int>> _dic;
    private Func<TMult, TMulti> _getSingle;
    private ValChangeAction<TMulti> _changedMult;
    public EntityMultiIndexer(Data data, Func<TMult, TMulti> getSingle,
        RefAction[] recalcTriggers,
        params ValChangeAction<TMulti>[] changeTriggers) : base(data)
    {
        _dic = new Dictionary<int, HashSet<int>>();
        _getSingle = getSingle;
        _changedMult = new ValChangeAction<TMulti>();
        _changedMult.Subscribe(n => 
        {
            if (n.OldVal != null && _dic.TryGetValue(n.OldVal.Id, out var hash))
            {
                hash.Remove(n.Entity.Id);
            }
            _dic.AddOrUpdate(n.NewVal.Id, n.Entity.Id);
        });
        foreach (var recalcTrigger in recalcTriggers)
        {
            recalcTrigger.Subscribe(() => Recalc(data));
        }
        foreach (var changeTrigger in changeTriggers)
        {
            changeTrigger.Subscribe(_changedMult);
        }
        data.SubscribeForDestruction<TMulti>(HandleTSingleRemoved);
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
        if(single != null) _dic.AddOrUpdate(single.Id, added.Id);
    }

    private void HandleTSingleRemoved(EntityDestroyedNotice n)
    {
        _dic.Remove(n.Entity.Id);
    }
    public override void HandleRemoved(TMult removing)
    {
        var single = _getSingle(removing);
        if(single != null) _dic[single.Id].Remove(removing.Id);
    }
}
