
using System;
using System.Collections.Generic;
using System.Linq;

public class ManyToManyIndexer
{
    public static ManyToManyIndexer<T, TIndex> 
        MakeForEntity<TIndex, T>
        (Func<T, IEnumerable<TIndex>> getIndices, Data d)
        where T : Entity
    {
        var res = new ManyToManyIndexer<T, TIndex>(getIndices,
            () => d.GetAll<T>());
        d.SubscribeForCreation<T>(n => res.HandleAdded((T)n.Entity));
        d.SubscribeForDestruction<T>(n => res.HandleRemoved((T)n.Entity));
        return res;
    }
}

public class ManyToManyIndexer<T, TIndex>
{
    public IReadOnlyCollection<T> this[TIndex s] => _dic.ContainsKey(s) 
        ? _dic[s] 
        : null;
    protected Dictionary<TIndex, HashSet<T>> _dic;
    private Func<T, IEnumerable<TIndex>> _getIndices;
    private Func<IEnumerable<T>> _getAll;
    
    public ManyToManyIndexer(
        Func<T, IEnumerable<TIndex>> getIndices,
        Func<IEnumerable<T>> getAll) 
    {
        _getAll = getAll;
        _dic = new Dictionary<TIndex, HashSet<T>>();
        _getIndices = getIndices;
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
    
    
    public void HandleAdded(T added)
    {
        var indices = _getIndices(added);
        foreach (var index in indices)
        {
            _dic.AddOrUpdate(index, added);
        }
    }
    public void HandleRemoved(T removing)
    {
        var indices = _getIndices(removing);
        foreach (var index in indices)
        {
            _dic[index].Remove(removing);
            if (_dic[index].Any() == false) _dic.Remove(index);
        }
    }

    public void HandleIndexAdd(T t, TIndex index)
    {
        _dic.AddOrUpdate(index, t);
    }
    public void HandleIndexRemove(T t, TIndex index)
    {
        _dic[index].Remove(t);
        if (_dic[index].Any() == false) _dic.Remove(index);
    }
    
    
    public void RegisterReCalc(RefAction action)
    {
        action.Subscribe(ReCalc);
    }
} 