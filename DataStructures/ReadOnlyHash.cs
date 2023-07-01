
using System.Collections;
using System.Collections.Generic;

public class ReadOnlyHash<T> : IReadOnlyHash<T>
{
    private HashSet<T> _hash;
    public ReadOnlyHash(HashSet<T> hash)
    {
        _hash = hash;
    }

    public bool Contains(T t) => _hash.Contains(t);
    public IEnumerator<T> GetEnumerator()
    {
        return _hash.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable) _hash).GetEnumerator();
    }

    public int Count => _hash.Count;
}

public static class ReadOnlyHashExt
{
    public static ReadOnlyHash<T> ReadOnly<T>(this HashSet<T> hash)
    {
        return new ReadOnlyHash<T>(hash);
    }
}