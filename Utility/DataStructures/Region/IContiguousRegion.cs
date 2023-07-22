
using System;
using System.Collections.Generic;

public interface IContiguousRegion<TNode> 
{
    Action<IEnumerable<IContiguousRegion<TNode>>> Split { get; set; }
    IRegionBoundary<TNode> Border { get; }
    IReadOnlyHash<IContiguousRegion<TNode>> K1Regions { get; }
    IReadOnlyGraph<IContiguousRegion<TNode>, Snake<TNode>> Bridges { get; }
    IReadOnlyHash<TNode> Elements { get; }
    void AddNode(TNode n);
    void RemoveNode(TNode n);
    bool IsBridgeElement(TNode node);
}
