    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Godot;

    public class Region<TNode> : IRegion<TNode>
    {
        public IReadOnlyGraph<TNode> Graph { get; }
        public IReadOnlyHash<TNode> Elements => _elements.ReadOnly();
        private HashSet<TNode> _elements;
        public IReadOnlyHash<IContiguousRegion<TNode>> ContiguousRegions => _contiguous.ReadOnly();
        private HashSet<IContiguousRegion<TNode>> _contiguous;
        private Dictionary<IContiguousRegion<TNode>, Color> _contCols;
        public IReadOnlyDictionary<TNode, IContiguousRegion<TNode>> NodeRegions => _nodeDic;
        private Dictionary<TNode, IContiguousRegion<TNode>> _nodeDic;
        
        public Region(HashSet<TNode> elements, IReadOnlyGraph<TNode> graph)
        {
            Graph = graph;
            _nodeDic = new Dictionary<TNode, IContiguousRegion<TNode>>();
            _contiguous = new HashSet<IContiguousRegion<TNode>>();
            
            _contCols = new Dictionary<IContiguousRegion<TNode>, Color>();
            foreach (var c in _contiguous)
            {
                c.Split += rs => HandleSplit(c, rs);
            }

            _elements = new HashSet<TNode>();
            foreach (var e in elements)
            {
                AddElement(e);
            }
        }

        private void HandleSplit(IContiguousRegion<TNode> splitter, IEnumerable<IContiguousRegion<TNode>> newRegions)
        {
            RemoveContRegion(splitter);
            _contiguous.AddRange(newRegions);
            foreach (var r in newRegions)
            {
                foreach (var rElement in r.Elements)
                {
                    _nodeDic[rElement] = r;
                }
            }
        }

        private void RemoveContRegion(IContiguousRegion<TNode> cont)
        {
            _contiguous.Remove(cont);
            _contCols.Remove(cont);
        }
        private void AddContRegion(ContiguousRegion<TNode> cont)
        {
            _contiguous.Add(cont);
            _contCols.Add(cont, ColorsExt.GetRandomColor());
        }
        public void AddElement(TNode t)
        {
            _elements.Add(t);
            
            var ns = Graph.GetNeighbors(t);
            var adjRegions = ns
                .Where(n => _nodeDic.ContainsKey(n))
                .Select(n => _nodeDic[n]).ToHashSet();
            
            if (adjRegions.Count == 0)
            {
                // GD.Print("adding new region");
                var cont = ContiguousRegion<TNode>.Construct(t, Graph);
                AddContRegion(cont);
                _nodeDic.Add(t, cont);
            }
            else if (adjRegions.Count == 1)
            {
                // GD.Print("joining to existing region");
                var cont = adjRegions.First();
                cont.AddNode(t);
                _nodeDic.Add(t, cont);
            }
            else 
            {
                // GD.Print($"merging {adjRegions.Count} regions");
                foreach (var cont in adjRegions)
                {
                    _contiguous.Remove(cont);
                }

                var els = adjRegions.SelectMany(a => a.Elements).ToHashSet();
                els.Add(t);
                var newConts = ContiguousRegion<TNode>.GetRegions(Graph, els).ToList();
                if (newConts.Count() != 1) throw new Exception();
                var newCont = newConts.First();
                AddContRegion(newCont);
                foreach (var node in newCont.Elements)
                {
                    _nodeDic[node] = newCont;
                }
            }
            // PrintRegionCounts();
        }

        public void RemoveElement(TNode t)
        {
            _elements.Remove(t);
            var cont = _nodeDic[t];
            _nodeDic.Remove(t);
            cont.RemoveNode(t);
            if (cont.Elements.Count == 0)
            {
                RemoveContRegion(cont);
            }
            // PrintRegionCounts();
        }

        private void PrintRegionCounts()
        {
            GD.Print("region count " + _contiguous.Count);
            foreach (var c in _contiguous)
            {
                GD.Print(c.Elements.Count);
            }
        }
        public Color GetRegionColor(IContiguousRegion<TNode> r)
        {
            return _contCols[r];
        }
    }
