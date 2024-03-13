
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;

public static class DictionaryExt
{
   
    public static bool AddOrUpdateRange<TKey, TValue, TCol>(this Dictionary<TKey, TCol> dic,
        TKey key, params TValue[] vals) where TCol : ICollection<TValue>, new()
    {
        if (dic.ContainsKey(key))
        {
            dic[key].AddRange(vals);
            return true;
        }
        else
        {
            var col = new TCol();
            col.AddRange(vals);
            dic.Add(key, col);
            return false;
        }
    }
    public static bool AddOrUpdate<TKey, TValue, TCol>(this IDictionary<TKey, TCol> dic,
        TKey key, TValue val) where TCol : ICollection<TValue>, new()
    {
        if (dic.ContainsKey(key))
        {
            dic[key].Add(val);
            return true;
        }
        else
        {
            var col = new TCol();
            col.Add(val);
            dic.Add(key, col);
            return false;
        }
    }
    public static bool AddOrSum<TKey>(this Dictionary<TKey, int> dic,
        TKey key, int val, int min = int.MinValue, int max = int.MaxValue)
    {
        if (dic.ContainsKey(key))
        {
            dic[key] = Mathf.Clamp(val + dic[key], min, max);
            return true;
        }
        else
        {
            dic[key] = Mathf.Clamp(val, min, max);
            return false;
        }
    }
    public static bool AddOrSum<TKey>(this Dictionary<TKey, float> dic,
        TKey key, float val, float min = float.MinValue, float max = float.MaxValue)
    {
        if (dic.ContainsKey(key))
        {
            dic[key] = Mathf.Clamp(val + dic[key], min, max);
            return true;
        }
        else
        {
            dic[key] = Mathf.Clamp(val, min, max);
            return false;
        }
    }

    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, Func<TKey, TValue> generate)
    {
        if (dic.TryGetValue(key, out var val)) return val;
        var genned = generate(key);
        dic.Add(key, genned);
        return genned;
    }

    public static Dictionary<TValue, int> GetCountsBy<TValue, TEnum>(
        this IEnumerable<TEnum> e, Func<TEnum, TValue> getValue)
    {
        var res = new Dictionary<TValue, int>();
        foreach (var t in e)
        {
            res.AddOrSum(getValue(t), 1);
        }

        return res;
    }
}
