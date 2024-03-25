using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class Sorter
{
    
}

public static class SorterExt
{
    public static Dictionary<TKey, List<TSource>> SortBy<TKey, TSource>(this IEnumerable<TSource> vals,
        Func<TSource, TKey> getKey)
    {
        var dic = new Dictionary<TKey, List<TSource>>();
        foreach (var val in vals)
        {
            var key = getKey(val);
            dic.AddOrUpdate(key, val);
        }
        return dic;
    }
    public static Dictionary<TKey, List<TValue>> SortBy<TKey, TSource, TValue>(this IEnumerable<TSource> vals,
        Func<TSource, TKey> getKey, Func<TSource, TValue> getValue)
    {
        var dic = new Dictionary<TKey, List<TValue>>();
        foreach (var source in vals)
        {
            var key = getKey(source);
            var val = getValue(source);
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
