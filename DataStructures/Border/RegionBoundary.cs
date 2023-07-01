    using System.Collections.Generic;
    using System.Linq;
    using Godot;

    public class RegionBoundary<TNode> : Boundary<TNode>, IRegionBoundary<TNode>
    {
        public IContiguousRegion<TNode> Region { get; }
        public RegionBoundary(IReadOnlyGraph<TNode> graph, IContiguousRegion<TNode> region) 
            : base(graph, region.Elements)
        {
            Region = region;
        }
    }