
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
        dic.Add(key, generate(key));
        return dic[key];
    }
    
    public static Dictionary<TKey, TValue> BindDictionary<TKey, TValue>(this List<TKey> keys,
        List<TValue> values)
    {
        if (keys.Count != values.Count) throw new Exception("different number of keys and values");
        var res = new Dictionary<TKey, TValue>();
        for (var i = 0; i < keys.Count; i++)
        {
            res.Add(keys[i], values[i]);
        }
        return res;
    }
    public static Dictionary<TKey, int> GetCounts<TKey, TIn>(this IEnumerable<TIn> ts, 
        Func<TIn, Dictionary<TKey, int>> getNums, Func<TIn, int, int> numMod)
    {
        var res = new Dictionary<TKey, int>();
        foreach (var t in ts)
        {
            var nums = getNums(t);
            foreach (var kvp2 in nums)
            {
                var item = kvp2.Key;
                var numItem = numMod(t, kvp2.Value);
                res.AddOrSum(item, numItem);
            }
        }

        return res;
    }
    public static Dictionary<TKey, int> GetCounts<TKey, TIn>(this IEnumerable<TIn> ts, 
        Func<TIn, Dictionary<TKey, int>> getNums)
    {
        var res = new Dictionary<TKey, int>();
        foreach (var t in ts)
        {
            var nums = getNums(t);
            foreach (var kvp2 in nums)
            {
                var item = kvp2.Key;
                var numItem = kvp2.Value;
                res.AddOrSum(item, numItem);
            }
        }

        return res;
    }
}
