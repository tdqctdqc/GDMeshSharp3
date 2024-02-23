using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class NewGraph<TNode, TEdge> 
{
    public Dictionary<TNode, Dictionary<TNode, TEdge>> Neighbors { get; private set; }

    public IEnumerable<TNode> Elements => Neighbors.Keys;

    public NewGraph()
    {
        Neighbors = new Dictionary<TNode, Dictionary<TNode, TEdge>>();
    }
    public bool HasEdge(TNode from, TNode to)
    {
        return Neighbors.TryGetValue(from, out var ns)
               && ns.ContainsKey(to);
    }

    public bool HasNode(TNode value)
    {
        return Neighbors.ContainsKey(value);
    }

    public IEnumerable<TNode> GetNeighbors(TNode value)
    {
        return Neighbors[value].Keys;
    }

    public TEdge GetEdge(TNode from, TNode to)
    {
        return Neighbors[from][to];
    }


    public void SetEdgeValue(TNode t1, TNode t2, TEdge newEdgeVal)
    {
        Neighbors[t1][t2] = newEdgeVal;
        Neighbors[t2][t1] = newEdgeVal;
    }

    public void AddEdge(TNode t1, TNode t2, TEdge edge)
    {
        Neighbors[t1].Add(t2, edge);
        Neighbors[t2].Add(t1, edge);
    }

    public void AddNode(TNode element)
    {
        Neighbors.Add(element, new Dictionary<TNode, TEdge>());
    }

    public void Remove(TNode value)
    {
        foreach (var kvp in Neighbors[value])
        {
            Neighbors[kvp.Key].Remove(value);
        }

        Neighbors.Remove(value);
    }
    public void ForEachEdge(Action<TNode, TNode, TEdge> action)
    {
        foreach (var (key, value) in Neighbors)
        {
            foreach (var (node, edge) in value)
            {
                action(key, node, edge);
            }
        }
    }
    
    public void ForEachEdgeParallel(Action<TNode, TNode, TEdge> action)
    {

        Parallel.ForEach(Neighbors, kvp =>
        {
            var node1 = kvp.Key;
            foreach (var (node2, value) in kvp.Value)
            {
                action(node1, node2, value);
            }
        });
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
        Neighbors[n1].Remove(n2);
        Neighbors[n2].Remove(n1);
    }
    
}