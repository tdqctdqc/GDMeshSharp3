using System;
using System.Collections.Generic;
using System.Linq;

public class MultiCountHistory<TKey>
{
    public Dictionary<TKey, CountHistory> Counts { get; private set; }
    public CountHistory this[TKey key] => Counts[key];
    public static MultiCountHistory<TKey> Construct()
    {
        return new MultiCountHistory<TKey>(new Dictionary<TKey, CountHistory>());
    }
    protected MultiCountHistory(Dictionary<TKey, CountHistory> counts)
    {
        Counts = counts;
    }
}
