
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;


public class ImplicitGraph
{
    public static ImplicitGraph<TNode, TEdge> Get<TNode, TEdge>(Func<IEnumerable<TNode>> getAllNodes,
        Func<IEnumerable<TEdge>> getAllEdges)
        where TNode : IReadOnlyGraphNode<TNode, TEdge>
    {
        return new ImplicitGraph<TNode, TEdge>(n => true, n => n.Neighbors, (n, m) => n.Neighbors.Contains(m),
            (n, m) => n.GetEdge(m), getAllNodes, getAllEdges);
    }
    
    public static ImplicitGraph<TNode, TEdge> Get<TNode, TEdge>(Func<TNode, IEnumerable<TNode>> getNeighbors,
        Func<TNode, TNode, TEdge> getEdge,
        Func<IEnumerable<TNode>> getAllNodes,
        Func<IEnumerable<TEdge>> getAllEdges)
    {
        return new ImplicitGraph<TNode, TEdge>(n => true, getNeighbors, 
            (n, m) => getNeighbors(n).Contains(m),
            getEdge, getAllNodes, getAllEdges);
    }
}
public class ImplicitGraph<TNode, TEdge> : IReadOnlyGraph<TNode, TEdge>
{
    private Func<TNode, bool> _contains;
    private Func<TNode, IEnumerable<TNode>> _getNeighbors;
    private Func<TNode, TNode, TEdge> _getEdge;
    private Func<TNode, TNode, bool> _hasEdge;
    private Func<IEnumerable<TNode>> _getAllNodes;
    private Func<IEnumerable<TEdge>> _getAllEdges;
    
    public ImplicitGraph(
        Func<TNode, bool> contains, 
        Func<TNode, IEnumerable<TNode>> getNeighbors, 
        Func<TNode, TNode, bool> hasEdge,
        Func<TNode, TNode, TEdge> getEdge,
        Func<IEnumerable<TNode>> getAllNodes,
        Func<IEnumerable<TEdge>> getAllEdges)
    {
        _getAllEdges = getAllEdges;
        _getAllNodes = getAllNodes;
        _contains = contains;
        _getNeighbors = getNeighbors;
        _getEdge = getEdge;
        _hasEdge = hasEdge;
    }


    public IEnumerable<TNode> Elements => _getAllNodes();
    public IEnumerable<TEdge> Edges => _getAllEdges();
    public bool HasEdge(TNode t1, TNode t2)
    {
        return _hasEdge(t1, t2);
    }

    public TEdge GetEdge(TNode from, TNode to)
    {
        return _getEdge(from, to);
    }

    public bool HasNode(TNode value)
    {
        return _contains(value);
    }

    public IEnumerable<TNode> GetNeighbors(TNode value)
    {
        return _getNeighbors(value);
    }
}
