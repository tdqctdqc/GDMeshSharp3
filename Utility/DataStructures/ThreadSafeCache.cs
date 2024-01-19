
using System;
using System.Collections.Concurrent;

public abstract class ThreadSafeCache<TKey, TValue>
{
    public ConcurrentDictionary<TKey, TValue> Dic { get; private set; }
    protected abstract TValue Make(TKey key);
    public ThreadSafeCache()
    {
        Dic = new ConcurrentDictionary<TKey, TValue>();
    }

    public void Clear()
    {
        Dic.Clear();
    }
    public TValue GetOrAdd(TKey key)
    {
        if (Dic.TryGetValue(key, out var value))
        {
            return value;
        }
        else
        {
            var newVal = Make(key);
            TryAdd(key, newVal);
            return newVal;
        }
    }
    private async void TryAdd(TKey key, TValue value)
    {
        Dic.TryAdd(key, value);
    }
}