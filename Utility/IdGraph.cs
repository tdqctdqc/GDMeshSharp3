using System.Collections.Generic;
using Godot;
using MessagePack;

namespace GDMeshSharp3.Utility;

public class IdGraph<T, V> where T : IIdentifiable
{
    public Dictionary<Vector2, V> Dic { get; private set; }
    public V this[T t1, T t2] => Contains(t1, t2) ? Get(t1, t2) : default;

    public static IdGraph<T, V> Construct()
    {
        return new IdGraph<T, V>(new Dictionary<Vector2, V>());
    }
    [SerializationConstructor] private IdGraph(Dictionary<Vector2, V> dic)
    {
        Dic = dic;
    }
    public bool TryAdd(T t1, T t2, V v)
    {
        return Dic.TryAdd(t1.GetIdEdgeKey(t2), v);
    }
    public void AddOrReplace(T t1, T t2, V v)
    {
        Dic.Add(t1.GetIdEdgeKey(t2), v);
    }
    public void Remove(T t1, T t2)
    {
        Dic.Remove(t1.GetIdEdgeKey(t2));
    }
    public bool Contains(T t1, T t2)
    {
        return Dic.ContainsKey(t1.GetIdEdgeKey(t2));
    }

    public V Get(T t1, T t2)
    {
        return Dic[t1.GetIdEdgeKey(t2)];
    }
}