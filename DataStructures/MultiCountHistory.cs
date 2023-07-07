using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class MultiCountHistory<TKey>
{
    public Dictionary<TKey, CountHistory> Counts { get; private set; }
    public CountHistory this[TKey key] => Counts.ContainsKey(key) ? Counts[key] : default;
    public static MultiCountHistory<TKey> Construct()
    {
        return new MultiCountHistory<TKey>(new Dictionary<TKey, CountHistory>());
    }
    [SerializationConstructor] public MultiCountHistory(Dictionary<TKey, CountHistory> counts)
    {
        Counts = counts;
    }
}
