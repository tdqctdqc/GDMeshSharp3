
using System;
using System.Collections.Generic;
using System.Linq;


public static class OneToManyIndexer
{
    public static OneToManyIndexer<TIndex, T> MakeForEntity<TIndex, T>
        (Func<T, TIndex> getSingle, Data d)
            where T : Entity
    {
        var res = new OneToManyIndexer<TIndex, T>(getSingle,
            () => d.GetAll<T>());
        d.SubscribeForCreation<T>(res.HandleAdded);
        d.SubscribeForDestruction<T>(res.HandleRemoved);
        return res;
    }
}

public class OneToManyIndexer<TIndex, T>
{
    public IReadOnlyCollection<T> this[TIndex s] => _dic.ContainsKey(s) 
        ? _dic[s]
        : null;
    protected Dictionary<TIndex, HashSet<T>> _dic;
    private Func<T, TIndex> _getSingle;
    private Func<IEnumerable<T>> _getAll;

    public OneToManyIndexer(Func<T, TIndex> getSingle,
        Func<IEnumerable<T>> getAll) 
    {
        _getAll = getAll;
        _dic = new Dictionary<TIndex, HashSet<T>>();
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
    public void RegisterAdd(RefAction<T> added)
    {
        added.Subscribe(HandleAdded);
    }
    public void RegisterRemove(RefAction<T> removed)
    {
        removed.Subscribe(HandleRemoved);
    }
    public void RegisterChanged(ValChangeAction<T, TIndex> changed)
    {
        changed.Subscribe(HandleChanged);
    }
    
    public void HandleAdded(T added)
    {
        var single = _getSingle(added);
        if(single != null) _dic.AddOrUpdate(single, added);
    }
    public void HandleRemoved(T removing)
    {
        var single = _getSingle(removing);
        if(single != null) _dic[single].Remove(removing);
        if (_dic[single].Any() == false) _dic.Remove(single);
    }

    public void HandleChanged(ValChangeNotice<T, TIndex> notice)
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

    public IReadOnlyDictionary<TIndex, HashSet<T>> GetDictionary()
    {
        return _dic;
    }
}