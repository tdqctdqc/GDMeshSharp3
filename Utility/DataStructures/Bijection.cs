using System.Collections.Generic;

public class Bijection<T1, T2>
{
    public T2 this[T1 t1] => _dic1[t1];
    public T1 this[T2 t2] => _dic2[t2];
    private Dictionary<T1, T2> _dic1;
    private Dictionary<T2, T1> _dic2;
    public IEnumerable<T2> Values => _dic1.Values;
    public Bijection()
    {
        _dic1 = new Dictionary<T1, T2>();
        _dic2 = new Dictionary<T2, T1>();
    }

    public void Add(T1 t1, T2 t2)
    {
        _dic1.Add(t1, t2);
        _dic2.Add(t2, t1);
    }

    public bool Contains(T1 t1)
    {
        return _dic1.ContainsKey(t1);
    }
    public bool Contains(T2 t2)
    {
        return _dic2.ContainsKey(t2);
    }

    public bool TryGetValue(T1 key, out T2 val)
    {
        if (_dic1.ContainsKey(key))
        {
            val = _dic1[key];
            return true;
        }

        val = default;
        return false;
    }
}