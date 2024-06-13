using System.Collections.Generic;
using MessagePack;

public class RefSet<T> where T : IdRef
{
    public HashSet<T> Items { get; private set; }

    [SerializationConstructor] protected RefSet(HashSet<T> items)
    {
        Items = items;
    }

    protected void Add(T t, StrongWriteKey key)
    {
        Items.Add(t);
    }
    protected void Remove(T t, StrongWriteKey key)
    {
        Items.Remove(t);
    }

    public int Count()
    {
        return Items.Count;
    }

    public bool Contains(T t)
    {
        return Items.Contains(t);
    }
}