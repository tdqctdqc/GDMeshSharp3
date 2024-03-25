
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class ConcurrentIdMultiEdgeGraph<T, V> where T : IIdentifiable
{
    public ConcurrentDictionary<Vector2I, ConcurrentDictionary<V, byte>> 
        EdgesByEdgeId { get; private set; }
    public ConcurrentDictionary<int, ConcurrentDictionary<int, byte>> 
        NodeNeighbors { get; private set; }

    public static ConcurrentIdMultiEdgeGraph<T, V> Construct()
    {
        return new ConcurrentIdMultiEdgeGraph<T, V>(new ConcurrentDictionary<Vector2I, ConcurrentDictionary<V, byte>>(),
            new ConcurrentDictionary<int, ConcurrentDictionary<int, byte>>());
    }
    [SerializationConstructor] public ConcurrentIdMultiEdgeGraph(
        ConcurrentDictionary<Vector2I, ConcurrentDictionary<V, byte>> edgesByEdgeId, 
        ConcurrentDictionary<int, ConcurrentDictionary<int, byte>> nodeNeighbors)
    {
        EdgesByEdgeId = edgesByEdgeId;
        NodeNeighbors = nodeNeighbors;
    }
    
    public void TryAddNode(T n)
    {
        if (NodeNeighbors.ContainsKey(n.Id)) return;
        NodeNeighbors.TryAdd(n.Id, new ConcurrentDictionary<int, byte>());
    }
    public ConcurrentDictionary<V, byte> GetEdge(T n1, T n2)
    {
        return EdgesByEdgeId[n1.GetIdEdgeKey(n2)];
    }

    public bool TryGetEdges(T t1, T t2, out ConcurrentDictionary<V, byte> edges)
    {
        var key = t1.GetIdEdgeKey(t2);
        if (EdgesByEdgeId.ContainsKey(key))
        {
            edges = EdgesByEdgeId[key];
            return true;
        }

        edges = null;
        return false;
    }
    public void AddToEdge(T t1, T t2, V edge)
    {
        TryAddNode(t1);
        TryAddNode(t2);
        var edgeId = t1.GetIdEdgeKey(t2);
        EdgesByEdgeId.GetOrAdd(edgeId, i => new ConcurrentDictionary<V, byte>())
            .TryAdd(edge, 0);
        NodeNeighbors[t1.Id].TryAdd(t2.Id, 0);
        NodeNeighbors[t2.Id].TryAdd(t1.Id, 0);
    }

    public void DoForEdges(T t, Action<int, V> act)
    {
        foreach (var n in NodeNeighbors[t.Id])
        {
            var key = t.GetIdEdgeKey(n.Key);
            var edge = EdgesByEdgeId[key];
            foreach (var v in edge)
            {
                act(n.Key, v.Key);
            }
        }
    }
    public IEnumerable<int> GetNeighborsWith(T t, 
        Func<V, bool> match)
    {
        TryAddNode(t);
        return NodeNeighbors[t.Id]
            .Keys
            .Where(n => EdgesByEdgeId.ContainsKey(t.GetIdEdgeKey(n))
                && EdgesByEdgeId[t.GetIdEdgeKey(n)].Any(v => match(v.Key)));
    }

    public void Remove(T t)
    {
        foreach (var n in NodeNeighbors[t.Id].ToList())
        {
            var key = t.GetIdEdgeKey(n.Key);
            EdgesByEdgeId.Remove(key, out _);
            NodeNeighbors[n.Key].Remove(t.Id, out _);
        }

        NodeNeighbors.Remove(t.Id, out _);
    }
}