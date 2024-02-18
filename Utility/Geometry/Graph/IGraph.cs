
using System.Collections.Generic;

public interface IGraph<TNode, TEdge>
    : IReadOnlyGraph<TNode, TEdge>
{
    void SetEdgeValue(TNode t1, TNode t2, TEdge newEdgeVal);
    void AddEdge(TNode t1, TNode t2, TEdge edge);
    void AddNode(TNode element);
    void Remove(TNode value);
}