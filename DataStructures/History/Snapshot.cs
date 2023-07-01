using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class Snapshot<TElement, TData>
{
    public Dictionary<TElement, TData> Entries { get; private set; }
    public static Snapshot<TElement, TData> Construct(Dictionary<TElement, TData> dic)
    {
        return new Snapshot<TElement, TData>(dic.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
    }
    public static Snapshot<TElement, TData> Construct()
    {
        return new Snapshot<TElement, TData>(new Dictionary<TElement, TData>());
    }
    [SerializationConstructor] private Snapshot(Dictionary<TElement, TData> entries)
    {
        Entries = entries;
    }
}
