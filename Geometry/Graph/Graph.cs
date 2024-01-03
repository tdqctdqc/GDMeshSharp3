using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Graph<TNode, TEdge> : IGraph<TNode, TEdge>
{
    public IGraphNode<TNode, TEdge> this[TNode t] => _nodeDic[t];
    private Dictionary<TNode, IGraphNode<TNode, TEdge>> _nodeDic;
    public List<TNode> Elements { get; private set; }
    public List<GraphNode<TNode, TEdge>> Nodes { get; private set; }
    public HashSet<TEdge> Edges { get; private set; }
    public Graph()
    {
        _nodeDic = new Dictionary<TNode, IGraphNode<TNode, TEdge>>();
        Elements = new List<TNode>();
        Nodes = new List<GraphNode<TNode, TEdge>>();
        Edges = new HashSet<TEdge>();
    }
    
    public bool HasEdge(TNode t1, TNode t2)
    {
        if (_nodeDic.ContainsKey(t1) == false) return false;
        return _nodeDic[t1].HasNeighbor(t2);
    }

    public void RemoveEdge(TNode t1, TNode t2)
    {
        var edge = GetEdge(t1, t2);
        var n1 = _nodeDic[t1];
        var n2 = _nodeDic[t2];
        n1.RemoveNeighbor(t2);
        n2.RemoveNeighbor(t1);
        Edges.Remove(edge);
    }

    public void RemoveEdges(Func<TEdge, bool> remove)
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

    IEnumerable<TNode> IReadOnlyGraph<TNode>.GetNeighbors(TNode value)
    {
        return GetNeighbors(value);
    }

    public IGraphNode<TNode> GetNode(TNode t)
    {
        return _nodeDic[t];
    }

    public void SetEdgeValue(TNode t1, TNode t2, TEdge newEdgeVal)
    {
        var node1 = _nodeDic[t1];
        var node2 = _nodeDic[t2];
        node1.SetEdgeValue(node2, newEdgeVal);
        node2.SetEdgeValue(node1, newEdgeVal);
    }
    public void AddEdge(TNode t1, TNode t2, TEdge edge)
    {
        if (t1 == null || t2 == null) return;
        if(_nodeDic.ContainsKey(t1) == false) AddNode(t1);
        if(_nodeDic.ContainsKey(t2) == false) AddNode(t2);
        AddUndirectedEdge(t1, t2, edge);
    }
    public void AddDirectedEdge(GraphNode<TNode, TEdge> from, 
        GraphNode<TNode, TEdge> to, TEdge edge)
    {
        Edges.Add(edge);
        from.AddNeighbor(to, edge);
    }
    private void AddUndirectedEdge(TNode from, TNode to, TEdge edge)
    {
        Edges.Add(edge);
        var fromNode = _nodeDic[from];
        var toNode = _nodeDic[to];
        fromNode.AddNeighbor(to, edge);
        toNode.AddNeighbor(from, edge);
    }
    public void AddDirectedEdge(TNode from, TNode to, TEdge edge)
    {
        Edges.Add(edge);
        var fromNode = _nodeDic[from];
        fromNode.AddNeighbor(to, edge);
    }
    public TEdge GetEdge(TNode t1, TNode t2)
    {
        var node1 = _nodeDic[t1];
        return node1.GetEdge(t2);
    }
    public void AddNode(GraphNode<TNode, TEdge> node)
    {
        _nodeDic.Add(node.Element, node);
        Elements.Add(node.Element);
        Nodes.Add(node);
    }
    public IGraphNode<TNode, TEdge> AddNode(TNode element)
    {
        var node = new GraphNode<TNode, TEdge>(element);
        _nodeDic.Add(node.Element, node);
        Elements.Add(element);
        Nodes.Add(node);
        return node;
    }
    
    public void AddUndirectedEdge(IGraphNode<TNode, TEdge> from, 
        IGraphNode<TNode, TEdge> to, TEdge edge)
    {
        Edges.Add(edge);
        from.AddNeighbor(to.Element, edge);
        to.AddNeighbor(from.Element, edge);
    }
    public bool Contains(TNode value)
    {
        return _nodeDic.ContainsKey(value);
    }
    public bool Remove(TNode value)
    {
        IGraphNode<TNode, TEdge> nodeToRemove = _nodeDic[value];
        if (nodeToRemove == null) return false;
        Elements.Remove(value);
        _nodeDic.Remove(nodeToRemove.Element);

        foreach (var neighbor in nodeToRemove.Neighbors)
        {
            var nNode = _nodeDic[neighbor];
            nNode.RemoveNeighbor(nodeToRemove.Element);
        }
        return true;
    }

    public HashSet<TNode> GetNeighbors(TNode value)
    {
        //todo make igraphnode neighbors hashset?
        return _nodeDic[value].Neighbors.ToHashSet();
    }
    public bool HasNode(TNode value)
    {
        return _nodeDic.ContainsKey(value);
    }
    IEnumerable<TNode> IReadOnlyGraph<TNode>.Elements => Elements;
    IEnumerable<TEdge> IReadOnlyGraph<TNode, TEdge>.Edges => Edges;
}