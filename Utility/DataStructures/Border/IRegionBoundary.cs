
    public interface IRegionBoundary<TNode> : IBoundary<TNode>
    {
        IContiguousRegion<TNode> Region { get; } 
    }
