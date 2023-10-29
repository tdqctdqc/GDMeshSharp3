using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;
public class History<TValue> where TValue : class
{
    public Dictionary<int, TValue> Dic { get; private set; }
    public int LatestTick { get; private set; }
    public static History<TValue> Construct()
    {
        return new History<TValue>(new Dictionary<int, TValue>(), -1);
    }
    [SerializationConstructor] private History(Dictionary<int, TValue> dic, int latestTick)
    {
        Dic = dic;
        LatestTick = latestTick;
    }
    public TValue Get(int tick) => Dic[tick];
    public void Add(int tick, TValue val)
    {
        if (LatestTick >= tick)
        {
            // throw new Exception($"adding to history out of order adding {tick} over {LatestTick}");
        }
        LatestTick = tick;
        
        Dic.Add(tick, val);
    }

    public TValue GetLatest()
    {
        return Get(LatestTick);
    }
    public List<TValue> GetOrdered()
    {
        return Dic.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
    }
}
public class History<TValue, TKey> where TKey : IIdentifiable where TValue : class
{
    public TValue Get(TKey k1, int tick) => Dic[new Vector2(k1.Id, tick)];
    public Dictionary<Vector2, TValue> Dic { get; private set; }
    public Dictionary<int, int> LatestTickByKeyId { get; private set; }
    public static History<TValue, TKey> Construct()
    {
        return new History<TValue, TKey>(new Dictionary<Vector2, TValue>(), new Dictionary<int, int>());
    }
    [SerializationConstructor] private History(Dictionary<Vector2, TValue> dic,
        Dictionary<int, int> latestTickByKeyId)
    {
        Dic = dic;
        LatestTickByKeyId = latestTickByKeyId;
    }

    public void Add(TKey k1, int tick, TValue val)
    {
        if (LatestTickByKeyId.ContainsKey(k1.Id))
        {
            if (LatestTickByKeyId[k1.Id] > tick) throw new Exception("adding to history out of order");
        }
        LatestTickByKeyId[k1.Id] = tick;
        
        var key = new Vector2(k1.Id, tick);
        Dic.Add(key, val);
    }

    public TValue GetLatest(TKey k)
    {
        if (LatestTickByKeyId.ContainsKey(k.Id))
        {
            return Get(k, LatestTickByKeyId[k.Id]);
        }
        return null;
    }
    public List<TValue> GetOrdered(TKey k1)
    {
        return Dic.Where(kvp => kvp.Key.X == k1.Id)
            .OrderBy(kvp => kvp.Key.Y).Select(kvp => kvp.Value).ToList();
    }
}


public class History<TValue, TKey1, TKey2> where TKey1 : IIdentifiable where TKey2 : IIdentifiable
    where TValue : class
{
    public TValue Get(TKey1 k1, TKey2 k2, int tick) => Dic[new Vector3(k1.Id, k2.Id, tick)];
    public Dictionary<Vector3, TValue> Dic { get; private set; }
    public Dictionary<Vector2, int> LatestTickByKeyId { get; private set; }
    public static History<TValue, TKey1, TKey2> Construct()
    {
        return new History<TValue, TKey1, TKey2>(new Dictionary<Vector3, TValue>(), new Dictionary<Vector2, int>());
    }
    [SerializationConstructor] private History(Dictionary<Vector3, TValue> dic,
        Dictionary<Vector2, int> latestTickByKeyId)
    {
        Dic = dic;
        LatestTickByKeyId = latestTickByKeyId;
    }

    public void Add(TKey1 k1, TKey2 k2, int tick, TValue val)
    {
        var subKey = new Vector2(k1.Id, k2.Id);
        if (LatestTickByKeyId.ContainsKey(subKey))
        {
            if (LatestTickByKeyId[subKey] >= tick) throw new Exception("adding to history out of order");
        }
        LatestTickByKeyId[subKey] = tick;
        
        var key = new Vector3(k1.Id, k2.Id, tick);
        Dic.Add(key, val);
    }
    public void Remove(TKey1 k1, TKey2 k2, int tick)
    {
        var key = new Vector3(k1.Id, k2.Id, tick);
        Dic.Remove(key);
    }
    
    public TValue GetLatest(TKey1 k1, TKey2 k2)
    {
        if (LatestTickByKeyId.ContainsKey(new Vector2(k1.Id, k2.Id)))
        {
            return Get(k1, k2, LatestTickByKeyId[new Vector2(k1.Id, k2.Id)]);
        }
        return null;
    }

    public List<TValue> GetOrdered(TKey1 k1, TKey2 k2)
    {
        return Dic.Where(kvp => kvp.Key.X == k1.Id && kvp.Key.Y == k2.Id)
            .OrderBy(kvp => kvp.Key.Z).Select(kvp => kvp.Value).ToList();
    }
}
