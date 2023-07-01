
using System.Collections.Generic;

public interface IGraph<TNode, TEdge>
    : IReadOnlyGraph<TNode, TEdge>
{
    IGraphNode<TNode> GetNode(TNode t);
    void SetEdgeValue(TNode t1, TNode t2, TEdge newEdgeVal);
    void AddEdge(TNode t1, TNode t2, TEdge edge);
    IGraphNode<TNode, TEdge> AddNode(TNode element);
    bool Remove(TNode value);
}