


using System;
using System.Collections.Generic;

public static class OneToOneIndexer
{
    public static OneToOneIndexer<TIndex, T>
        MakeForEntity<TIndex, T>
            (Func<T, TIndex> getKey, Data d)
            where T : Entity
    {
        var indexer = new OneToOneIndexer<TIndex, T>(
            () => d.GetAll<T>(),
            getKey);
        d.SubscribeForCreation<T>(n => indexer.HandleAdded((T)n.Entity));
        d.SubscribeForDestruction<T>(n => indexer.HandleRemoved((T)n.Entity));
        return indexer;
    }
}

public class OneToOneIndexer<TIndex, T>
    where T : class
{
    private Func<T, TIndex> _getKey;
    private Func<IEnumerable<T>> _getAll;
    private Dictionary<TIndex, T> _dic;
    public T this[TIndex key] => 
        _dic.ContainsKey(key) 
            ? _dic[key] : null;

    public OneToOneIndexer(Func<IEnumerable<T>> getAll,
        Func<T, TIndex> getKey)
    {
        _getAll = getAll;
        _getKey = getKey;
        _dic = new Dictionary<TIndex, T>();
        foreach (var value in _getAll())
        {
            HandleAdded(value);
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

    public void RegisterReCalc(RefAction action)
    {
        action.Subscribe(ReCalc);
    }
    public void HandleAdded(T v)
    {
        var key = _getKey(v);
        if (key == null)
        {
            return;
        }
        _dic.Add(key, v);
    }
    public void HandleRemoved(T v)
    {
        _dic.Remove(_getKey(v));
    }

    public void HandleChanged(ValChangeNotice<T, TIndex> notice)
    {
        if (notice.OldVal != null)
        {
            _dic.Remove(notice.OldVal);
        }

        if (notice.NewVal != null)
        {
            _dic.Add(notice.NewVal, notice.Owner);
        }
    }

    public bool Contains(TIndex key)
    {
        return _dic.ContainsKey(key);
    }
}