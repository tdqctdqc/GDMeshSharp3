using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;
public class History<TValue> where TValue : class
{
    public TValue this[int tick] => _dic[tick];
    private Dictionary<int, TValue> _dic;
    private int _latest;
    public static History<TValue> Construct()
    {
        return new History<TValue>(new Dictionary<int, TValue>(), -1);
    }
    [SerializationConstructor] private History(Dictionary<int, TValue> dic, int latest)
    {
        _dic = dic;
        _latest = latest;
    }

    public void Add(int tick, TValue val)
    {
        if (_latest >= tick)
        {
            throw new Exception($"adding to history out of order adding {tick} over {_latest}");
        }
        _latest = tick;
        
        _dic.Add(tick, val);
    }

    public TValue Latest()
    {
        return this[_latest];
    }
    public List<TValue> GetOrdered()
    {
        return _dic.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
    }
}
public class History<TValue, TKey> where TKey : IIdentifiable where TValue : class
{
    public TValue this[TKey k1, int tick] => _dic[new Vector2(k1.Id, tick)];
    private Dictionary<Vector2, TValue> _dic;
    private Dictionary<int, int> _latestTickByKeyId;
    public static History<TValue, TKey> Construct()
    {
        return new History<TValue, TKey>(new Dictionary<Vector2, TValue>(), new Dictionary<int, int>());
    }
    [SerializationConstructor] private History(Dictionary<Vector2, TValue> dic,
        Dictionary<int, int> latestTickByKeyId)
    {
        _dic = dic;
        _latestTickByKeyId = latestTickByKeyId;
    }

    public void Add(TKey k1, int tick, TValue val)
    {
        if (_latestTickByKeyId.ContainsKey(k1.Id))
        {
            if (_latestTickByKeyId[k1.Id] >= tick) throw new Exception("adding to history out of order");
        }
        _latestTickByKeyId[k1.Id] = tick;
        
        var key = new Vector2(k1.Id, tick);
        _dic.Add(key, val);
    }

    public TValue Latest(TKey k)
    {
        if (_latestTickByKeyId.ContainsKey(k.Id))
        {
            return this[k, _latestTickByKeyId[k.Id]];
        }
        return null;
    }
    public List<TValue> GetOrdered(TKey k1)
    {
        return _dic.Where(kvp => kvp.Key.X == k1.Id)
            .OrderBy(kvp => kvp.Key.Y).Select(kvp => kvp.Value).ToList();
    }
}


public class History<TValue, TKey1, TKey2> where TKey1 : IIdentifiable where TKey2 : IIdentifiable
    where TValue : class
{
    public TValue this[TKey1 k1, TKey2 k2, int tick] => _dic[new Vector3(k1.Id, k2.Id, tick)];
    private Dictionary<Vector3, TValue> _dic;
    private Dictionary<Vector2, int> _latestTickByKeyId;
    public static History<TValue, TKey1, TKey2> Construct()
    {
        return new History<TValue, TKey1, TKey2>(new Dictionary<Vector3, TValue>(), new Dictionary<Vector2, int>());
    }
    [SerializationConstructor] private History(Dictionary<Vector3, TValue> dic,
        Dictionary<Vector2, int> latestTickByKeyId)
    {
        _dic = dic;
        _latestTickByKeyId = latestTickByKeyId;
    }

    public void Add(TKey1 k1, TKey2 k2, int tick, TValue val)
    {
        var subKey = new Vector2(k1.Id, k2.Id);
        if (_latestTickByKeyId.ContainsKey(subKey))
        {
            if (_latestTickByKeyId[subKey] >= tick) throw new Exception("adding to history out of order");
        }
        _latestTickByKeyId[subKey] = tick;
        
        var key = new Vector3(k1.Id, k2.Id, tick);
        _dic.Add(key, val);
    }
    public void Remove(TKey1 k1, TKey2 k2, int tick)
    {
        var key = new Vector3(k1.Id, k2.Id, tick);
        _dic.Remove(key);
    }
    
    public TValue Latest(TKey1 k1, TKey2 k2)
    {
        if (_latestTickByKeyId.ContainsKey(new Vector2(k1.Id, k2.Id)))
        {
            return this[k1, k2, _latestTickByKeyId[new Vector2(k1.Id, k2.Id)]];
        }
        return null;
    }

    public List<TValue> GetOrdered(TKey1 k1, TKey2 k2)
    {
        return _dic.Where(kvp => kvp.Key.X == k1.Id && kvp.Key.Y == k2.Id)
            .OrderBy(kvp => kvp.Key.Z).Select(kvp => kvp.Value).ToList();
    }
}
