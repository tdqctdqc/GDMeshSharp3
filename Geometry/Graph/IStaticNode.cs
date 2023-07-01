
using System.Collections.Generic;
using System.Linq;

public interface IStaticNode<TNode, TEdge> : IStaticNode<TNode>
{
    IReadOnlyGraph<TNode, TEdge> Graph { get; }
}
public interface IStaticNode<TNode> : IGraphNode<TNode>
{
    TNode Element { get; }
    IReadOnlyGraph<TNode> Graph { get; }
}

public static class IStaticGraphNodeExt
{
    public static List<Segment<TNode>> GetOrderedBoundarySegs<TNode>(this IEnumerable<TNode> elements)
        where TNode : IStaticNode<TNode>
    {
        return elements.First().Graph.GetOrderedBoundarySegs(elements);
    }
    public static List<BorderEdge<TNode>> GetOrderedBorderPairs<TNode>(this IEnumerable<TNode> elements)
        where TNode : IStaticNode<TNode>
    {
        return elements.First().Graph.GetOrderedBorderPairs(elements);
    } 
}