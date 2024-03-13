
using System;
using System.Collections.Generic;
using System.Linq;


public static class MultiIndexer
{
    public static MultiIndexer<TSingle, TMult> MakeForEntity<TSingle, TMult>
        (Func<TMult, TSingle> getSingle, Data d)
            where TMult : Entity
    {
        var res = new MultiIndexer<TSingle, TMult>(getSingle,
            () => d.GetAll<TMult>());
        d.SubscribeForCreation<TMult>(n => res.HandleAdded((TMult)n.Entity));
        d.SubscribeForDestruction<TMult>(n => res.HandleRemoved((TMult)n.Entity));
        return res;
    }
}

public class MultiIndexer<TSingle, TMult>
{
    public List<TMult> this[TSingle s] => _dic.ContainsKey(s) 
        ? _dic[s].ToList() 
        : null;
    protected Dictionary<TSingle, HashSet<TMult>> _dic;
    private Func<TMult, TSingle> _getSingle;
    private Func<IEnumerable<TMult>> _getAll;

    public MultiIndexer(Func<TMult, TSingle> getSingle,
        Func<IEnumerable<TMult>> getAll) 
    {
        _getAll = getAll;
        _dic = new Dictionary<TSingle, HashSet<TMult>>();
        _getSingle = getSingle;
        foreach (var mult in _getAll())
        {
            HandleAdded(mult);
        }
    }
    public void ReCalc()
    {
        _dic.Clear();
        foreach (var value in _getAll())
        {
            HandleAdded(value);
        }
    }
    public void RegisterAdd(RefAction<TMult> added)
    {
        added.Subscribe(HandleAdded);
    }
    public void RegisterRemove(RefAction<TMult> removed)
    {
        removed.Subscribe(HandleRemoved);
    }
    public void RegisterChanged(ValChangeAction<TMult, TSingle> changed)
    {
        changed.Subscribe(HandleChanged);
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
        if (_dic[single].Count() == 0) _dic.Remove(single);
    }

    public void HandleChanged(ValChangeNotice<TMult, TSingle> notice)
    {
        var mult = notice.Owner;
        var oldSingle = notice.OldVal;
        if (oldSingle is not null)
        {
            _dic[oldSingle].Remove(mult);
        }
        HandleAdded(mult);
    }
    
    public void RegisterReCalc(RefAction action)
    {
        action.Subscribe(ReCalc);
    }
}