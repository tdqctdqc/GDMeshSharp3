using System;
using System.Collections.Generic;
using System.Linq;

public class NewGraph<TNode, TEdge> : IGraph<TNode, TEdge>
{
    private Dictionary<TNode, Dictionary<TNode, TEdge>> _neighbors;

    public IEnumerable<TNode> Elements => _neighbors.Keys;
    public bool HasEdge(TNode from, TNode to)
    {
        return _neighbors.TryGetValue(from, out var ns)
               && ns.ContainsKey(to);
    }

    public bool HasNode(TNode value)
    {
        return _neighbors.ContainsKey(value);
    }

    public IEnumerable<TNode> GetNeighbors(TNode value)
    {
        return _neighbors[value].Keys;
    }

    public TEdge GetEdge(TNode from, TNode to)
    {
        return _neighbors[from][to];
    }


    public void SetEdgeValue(TNode t1, TNode t2, TEdge newEdgeVal)
    {
        _neighbors[t1][t2] = newEdgeVal;
        _neighbors[t2][t1] = newEdgeVal;
    }

    public void AddEdge(TNode t1, TNode t2, TEdge edge)
    {
        _neighbors[t1].Add(t2, edge);
    }

    public void AddNode(TNode element)
    {
        _neighbors.Add(element, new Dictionary<TNode, TEdge>());
    }

    public void Remove(TNode value)
    {
        foreach (var kvp in _neighbors[value])
        {
            _neighbors[kvp.Key].Remove(value);
        }

        _neighbors.Remove(value);
    }
    
    public void ForEachEdge(Action<TNode, TNode, TEdge> action)
    {
        foreach (var (key, value) in _neighbors)
        {
            foreach (var (node, edge) in value)
            {
                action(key, node, edge);
            }
        }
    }
    
    public void RemoveEdgesWhere(Func<TEdge, bool> remove)
    {
        foreach (var el in Elements)
        {
            var ns = GetNeighbors(el).ToArray();
            foreach (var n in ns)
            {
                var edge = GetEdge(el, n);
                if (remove(edge))
                {
                    RemoveEdge(el, n);
                }
            }
        }
    }

    public void RemoveEdge(TNode n1, TNode n2)
    {
        _neighbors[n1].Remove(n2);
        _neighbors[n2].Remove(n1);
    }
    
    
}