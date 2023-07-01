
using System.Collections.Generic;

public interface IReadOnlyGraphNode<TNode>
{
    TNode Element { get; }
    IReadOnlyCollection<TNode> Neighbors { get; }
    bool HasNeighbor(TNode neighbor);
}

public interface IReadOnlyGraphNode<TNode, TEdge> : IReadOnlyGraphNode<TNode>
{
    TEdge GetEdge(TNode neighbor);
}
