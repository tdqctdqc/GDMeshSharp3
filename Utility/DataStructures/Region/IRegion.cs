
using System;
using System.Collections.Generic;
using System.Linq;

public interface IRegion<TNode>
{
    IReadOnlyGraph<TNode> Graph { get; }
    IReadOnlyHash<TNode> Elements { get; }
    IReadOnlyHash<IContiguousRegion<TNode>> ContiguousRegions { get; }
    void RemoveElement(TNode t);
    void AddElement(TNode t);
}

public static class IRegionExt
{
        
}
