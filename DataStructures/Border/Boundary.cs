
    using System;
    using System.Collections.Generic;

    public class Boundary<TPrim> : Chain<Segment<TPrim>, TPrim>, IBoundary<TPrim>
    {
        public Action<TPrim> CrossedSelf { get; set; }
        public HashSet<TPrim> Elements { get; private set; }
        public IReadOnlyList<BorderEdge<TPrim>> OrderedBorderPairs => _orderedBorderPairs;
        protected List<BorderEdge<TPrim>> _orderedBorderPairs;
        protected Boundary(IReadOnlyGraph<TPrim> graph, IReadOnlyCollection<TPrim> innerElements) 
            : base(graph.GetOrderedBoundarySegs(innerElements))
        {
            _orderedBorderPairs = graph.GetOrderedBorderPairs(innerElements);
            Elements = innerElements.ToHashSet();
        }
    }
