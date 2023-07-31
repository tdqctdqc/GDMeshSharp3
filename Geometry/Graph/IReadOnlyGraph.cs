using System.Collections.Generic;
public interface IReadOnlyGraph<TNode>
{
    IEnumerable<TNode> Elements { get; }
    bool HasEdge(TNode from, TNode to);
    bool HasNode(TNode value);
    IEnumerable<TNode> GetNeighbors(TNode value);
}
public interface IReadOnlyGraph<TNode, TEdge> : IReadOnlyGraph<TNode>
{
    IEnumerable<TEdge> Edges { get; }
    TEdge GetEdge(TNode from, TNode to);
}
