using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class History<TValue>
{
    public Dictionary<int, TValue> ByTick { get; private set; }
    protected List<KeyValuePair<int, TValue>> _list;

    public static History<TValue> Construct()
    {
        return new History<TValue>(new Dictionary<int, TValue>());
    }
    [SerializationConstructor] public History(Dictionary<int, TValue> byTick)
    {
        ByTick = byTick;
        _list = byTick.ToList();
    }
    public void Add(TValue t, int tick)
    {
        ByTick.Add(tick, t);
        _list.Add(new KeyValuePair<int, TValue>(tick, t));
    }

    public TValue GetLatest()
    {
        if (_list.Count < 1) return default;
        return _list[_list.Count - 1].Value;
    }
    
}
