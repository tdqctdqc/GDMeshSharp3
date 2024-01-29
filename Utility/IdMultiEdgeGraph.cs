using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class IdMultiEdgeGraph<T, V> where T : IIdentifiable
{
    public Dictionary<Vector2I, HashSet<V>> EdgesByEdgeId { get; private set; }
    public Dictionary<int, HashSet<int>> NodeNeighbors { get; private set; }

    public static IdMultiEdgeGraph<T, V> Construct()
    {
        return new IdMultiEdgeGraph<T, V>(new Dictionary<Vector2I, HashSet<V>>(),
            new Dictionary<int, HashSet<int>>());
    }
    [SerializationConstructor] public IdMultiEdgeGraph(
        Dictionary<Vector2I, HashSet<V>> edgesByEdgeId, 
        Dictionary<int, HashSet<int>> nodeNeighbors)
    {
        EdgesByEdgeId = edgesByEdgeId;
        NodeNeighbors = nodeNeighbors;
    }
    
    public void TryAddNode(T n)
    {
        NodeNeighbors.Add(n.Id, new HashSet<int>());
    }
    public HashSet<V> GetEdge(T n1, T n2)
    {
        return EdgesByEdgeId[n1.GetIdEdgeKey(n2)];
    }

    public bool TryGetEdges(T t1, T t2, out HashSet<V> edges)
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
        EdgesByEdgeId.GetOrAdd(edgeId, i => new HashSet<V>())
            .Add(edge);
        NodeNeighbors[t1.Id].Add(t2.Id);
        NodeNeighbors[t2.Id].Add(t1.Id);
    }

    public void DoForEdges(T t, Action<int, V> act)
    {
        foreach (var n in NodeNeighbors[t.Id])
        {
            var key = t.GetIdEdgeKey(n);
            var edge = EdgesByEdgeId[key];
            foreach (var v in edge)
            {
                act(n, v);
            }
        }
    }
    public IEnumerable<int> GetNeighborsWith(T t, 
        Func<V, bool> match)
    {
        TryAddNode(t);
        return NodeNeighbors[t.Id]
            .Where(n => EdgesByEdgeId[t.GetIdEdgeKey(n)].Any(match));
    }

    public void Remove(T t)
    {
        foreach (var n in NodeNeighbors[t.Id])
        {
            var key = t.GetIdEdgeKey(n);
            EdgesByEdgeId.Remove(key);
        }

        NodeNeighbors.Remove(t.Id);
    }
}