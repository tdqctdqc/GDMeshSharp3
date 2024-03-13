using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class Sorter
{
    
}

public static class SorterExt
{
    public static Dictionary<TKey, List<TValue>> SortInto<TKey, TValue>(this IEnumerable<TValue> vals,
        Func<TValue, TKey> getKey)
    {
        var dic = new Dictionary<TKey, List<TValue>>();
        foreach (var val in vals)
        {
            var key = getKey(val);
            dic.AddOrUpdate(key, val);
        }
        return dic;
    }
    
    public static Dictionary<TKey, int> SortInto<TKey, TSource>(this IEnumerable<TSource> sources,
        Func<TSource, TKey> getKey, Func<TSource, int> getValue)
    {
        var dic = new Dictionary<TKey, int>();
        foreach (var source in sources)
        {
            var key = getKey(source);
            var val = getValue(source);
            dic.AddOrSum(key, val);
        }
        return dic;
    }
    
    public static Dictionary<TKey, float> SortInto<TKey, TSource>(this IEnumerable<TSource> sources,
        Func<TSource, TKey> getKey, Func<TSource, float> getValue)
    {
        var dic = new Dictionary<TKey, float>();
        foreach (var source in sources)
        {
            var key = getKey(source);
            var val = getValue(source);
            dic.AddOrSum(key, val);
        }
        return dic;
    }
}
