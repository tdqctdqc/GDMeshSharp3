


using System;
using System.Collections.Generic;

public static class Indexer
{
    public static Indexer<TKey, TValue>
        MakeForEntity<TKey, TValue>
            (Func<TValue, TKey> getKey, Data d)
            where TValue : Entity
    {
        var indexer = new Indexer<TKey, TValue>(() => d.GetAll<TValue>(),
            getKey);
        d.SubscribeForCreation<TValue>(n => indexer.HandleAdded((TValue)n.Entity));
        d.SubscribeForDestruction<TValue>(n => indexer.HandleRemoved((TValue)n.Entity));
        return indexer;
    }
}

public class Indexer<TKey, TValue>
    where TValue : class
{
    private Func<TValue, TKey> _getKey;
    private Func<IEnumerable<TValue>> _getAll;
    private Dictionary<TKey, TValue> _dic;
    public TValue this[TKey key] => _dic.ContainsKey(key) ? _dic[key] : null;

    public Indexer(Func<IEnumerable<TValue>> getAll,
        Func<TValue, TKey> getKey)
    {
        _getAll = getAll;
        _getKey = getKey;
        _dic = new Dictionary<TKey, TValue>();
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
    public void RegisterAdd(RefAction<TValue> added)
    {
        added.Subscribe(HandleAdded);
    }
    public void RegisterRemove(RefAction<TValue> removed)
    {
        removed.Subscribe(HandleRemoved);
    }
    public void RegisterChanged(ValChangeAction<TValue, TKey> changed)
    {
        changed.Subscribe(HandleChanged);
    }

    public void RegisterReCalc(RefAction action)
    {
        action.Subscribe(ReCalc);
    }
    public void HandleAdded(TValue v)
    {
        _dic.Add(_getKey(v), v);
    }
    public void HandleRemoved(TValue v)
    {
        _dic.Remove(_getKey(v));
    }

    public void HandleChanged(ValChangeNotice<TValue, TKey> notice)
    {
        _dic.Remove(notice.OldVal);
        _dic.Add(notice.NewVal, notice.Owner);
    }

    public bool Contains(TKey key)
    {
        return _dic.ContainsKey(key);
    }
}