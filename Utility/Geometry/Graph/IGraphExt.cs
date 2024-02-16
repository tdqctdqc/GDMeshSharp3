
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class IGraphExt
{
    public static IEnumerable<TSub> GetBorderElements<TSub>(this IReadOnlyGraph<TSub> graph,
        IEnumerable<TSub> els)
    {
        return els.Where(e => graph.GetNeighbors(e).Any(n => els.Contains(n) == false));
    }

    
    
    public static List<TEdge> GetBorderEdges<TEdge, TRegion>
        (this IReadOnlyGraph<TRegion, TEdge> graph, IEnumerable<TRegion> elements, Func<TEdge, TRegion> getEdgeNative,
            Func<TEdge, TRegion> getEdgeForeign)
    {
        var nativeHash = elements.ToHashSet().ReadOnly();
        var borderNodes = graph.GetBorderElements(nativeHash);
        var nativeEdgeDic = new Dictionary<TRegion, List<TEdge>>();
        var foreignEdgeDic = new Dictionary<TRegion, List<TEdge>>();
        
        foreach (var borderNode in borderNodes)
        {
            if (nativeEdgeDic.ContainsKey(borderNode))
            {
                continue;
            }

            var thisNodeEdges = graph.GetNeighbors(borderNode)
                .Where(foreign)
                .Select(foreignNode => graph.GetEdge(borderNode, foreignNode)).ToList();
            nativeEdgeDic.Add(borderNode, thisNodeEdges);
            foreach (var e in thisNodeEdges)
            {
                foreignEdgeDic.AddOrUpdate(getForeign(e), e);
            }
        }
        
        var firstSub = borderNodes.First();
        var firstEdges = nativeEdgeDic[firstSub];
        if (firstEdges.Count == 0) return null;
        var firstEdge = firstEdges[0];
        var firstEdgeNeighbors = getAdjEdges(firstEdge);
        
        var left = new List<TEdge>();
        var right = new List<TEdge>();
        var count = firstEdgeNeighbors.Count();
        var covered = new HashSet<TEdge> {firstEdge};
        
        if (count > 0)
        {
            traverse(firstEdgeNeighbors.ElementAt(0), left);
        }
        if (count > 1)
        {
            traverse(firstEdgeNeighbors.ElementAt(1), right);
        }

        var result = new List<TEdge>();
        for (int i = left.Count - 1; i >= 0; i--)
        {
            result.Add(left[i]);
        }
        result.Add(firstEdge);
        for (var i = 0; i < right.Count; i++)
        {
            result.Add(right[i]);
        }
        return result;
        
        IEnumerable<TEdge> getAdjEdges(TEdge edge)
        {
            return nativeEdgeDic[getNative(edge)].Union(foreignEdgeDic[getForeign(edge)])
                .Where(e => e.Equals(edge) == false && adjacentEdge(e, edge))
                .Distinct();
        }
        bool foreign(TRegion node)
        {
            return nativeHash.Contains(node) == false;
        }
        TRegion getForeign(TEdge e)
        {
            var n = getEdgeNative(e);
            var f = getEdgeForeign(e);
            if (foreign(n))
            {
                if (foreign(f)) throw new Exception();
                return n;
            }
            if (foreign(f)) return f;
            throw new Exception();
        }
        TRegion getNative(TEdge e)
        {
            var n = getEdgeNative(e);
            var f = getEdgeForeign(e);
            if (foreign(n) == false)
            {
                if (foreign(f) == false) throw new Exception();
                return n;
            }
            if (foreign(f) == false) return f;
            throw new Exception();
        }
        bool adjacentEdge(TEdge e1, TEdge e2)
        {
            return adjacentNode(e1, getNative(e2)) || adjacentNode(e1, getForeign(e2));
        }
        bool adjacentNode(TEdge e, TRegion s)
        {
            var ns = graph.GetNeighbors(s);
            return ns.Contains(getForeign(e)) && ns.Contains(getNative(e));
        }
        void traverse(TEdge e, List<TEdge> list)
        {
            list.Add(e);
            covered.Add(e);
            var adj = getAdjEdges(e).Where(a => covered.Contains(a) == false);
            if (adj.Count() > 1) throw new Exception();
            if (adj.Count() > 0)
            {
                traverse(adj.First(), list);
            }
        }
    }
    
    
    
}
