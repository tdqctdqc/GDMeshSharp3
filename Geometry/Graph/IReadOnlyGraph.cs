using System.Collections.Generic;
public interface IReadOnlyGraph<TNode>
{
    IReadOnlyCollection<TNode> Elements { get; }
    bool HasEdge(TNode from, TNode to);
    bool HasNode(TNode value);
    IReadOnlyCollection<TNode> GetNeighbors(TNode value);
}
public interface IReadOnlyGraph<TNode, TEdge> : IReadOnlyGraph<TNode>
{
    IReadOnlyCollection<TEdge> Edges { get; }
    TEdge GetEdge(TNode from, TNode to);
}
