
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ContiguousRegion<TNode> : IContiguousRegion<TNode>
    {
        public IReadOnlyHash<TNode> Elements => _elements.ReadOnly();
        private HashSet<TNode> _elements;
        public Action<IEnumerable<IContiguousRegion<TNode>>> Split { get; set; }
        //todo multiple borders!
        public IRegionBoundary<TNode> Border => _border;
        private RegionBoundary<TNode> _border;
        public IReadOnlyHash<IContiguousRegion<TNode>> K1Regions => _k1s.ReadOnly();
        private HashSet<IContiguousRegion<TNode>> _k1s;
        public IReadOnlyGraph<IContiguousRegion<TNode>, Snake<TNode>> Bridges => _bridges;
        private Graph<IContiguousRegion<TNode>, Snake<TNode>> _bridges;

        public static IEnumerable<ContiguousRegion<TNode>> GetRegions(IReadOnlyGraph<TNode> graph, 
            IEnumerable<TNode> elements)
        {
            var hash = new HashSet<TNode>(elements);
            var unions = UnionFind.Find<TNode>(graph, elements, (a,b) => hash.Contains(a) == hash.Contains(b));
            return unions.Select(u => new ContiguousRegion<TNode>(u, graph));
        }

        public static ContiguousRegion<TNode> Construct(TNode element, IReadOnlyGraph<TNode> graph)
        {
            return new ContiguousRegion<TNode>(new[] {element}, graph);
        }
        private ContiguousRegion(IEnumerable<TNode> elements, IReadOnlyGraph<TNode> graph)
        {
            _k1s = new HashSet<IContiguousRegion<TNode>>();
            _bridges = new Graph<IContiguousRegion<TNode>, Snake<TNode>>();
            _elements = new HashSet<TNode>();
            foreach (var e in elements)
            {
                AddNode(e);
            }
            _border = new RegionBoundary<TNode>(graph, this);
            _border.CrossedSelf += HandleBoundarySelfCross;
        }
        public void AddNode(TNode n)
        {
            //check if node adjacent to any bridges
            _elements.Add(n);
        }
        public void RemoveNode(TNode n)
        {
            //check if node part of any bridges
            _elements.Remove(n);
        }

        public bool IsBridgeElement(TNode node)
        {
            return Bridges.Edges.Any(e => e.Segments.Any(s => s.From.Equals(node) || s.To.Equals(node)));
        }
        private void HandleBoundarySelfCross(TNode cross)
        {
            throw new NotImplementedException();
        }
    }
