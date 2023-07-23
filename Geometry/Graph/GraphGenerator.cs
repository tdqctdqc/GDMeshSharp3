using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DelaunatorSharp;

public static class GraphGenerator
{
    public static Graph<MapPolygon, LineSegment> GenerateMapPolyVoronoiGraph
        (MapGenInfo info, GenWriteKey key)
    {
        var g = GenerateVoronoiGraph<MapPolygon, LineSegment>(
            info.Polys,
            mp => mp.Center,
            (v1, v2, mp1, mp2) =>
            {
                v1 = v1.ClampToBox(Vector2.Zero, info.Dimensions);
                v2 = v2.ClampToBox(Vector2.Zero, info.Dimensions);
                return new LineSegment(v1, v2);
            },
            new Vector2(key.GenData.Planet.Width, key.GenData.Planet.Height)
        );
        return g;
    }

    public static void WrapMapPolygonGraph(Graph<MapPolygon, LineSegment> graph,
        List<MapPolygon> keepMergePolys, List<MapPolygon> discardMergePolys, 
        GenWriteKey key)
    {
        for (var i = 0; i < keepMergePolys.Count - 1; i++)
        {
            var keep = keepMergePolys[i];
            var discard = discardMergePolys[i];
            var nextKeep = keepMergePolys[i + 1];
            var nextDiscard = discardMergePolys[i + 1];
            var discardEdge = graph.GetEdge(discard, nextDiscard);
            var keepEdge = graph.GetEdge(keep, nextKeep);
            var newPoints = new List<Vector2> {keepEdge.From, keepEdge.To, discardEdge.From, discardEdge.To}
                .Where(p => p.X != 0 
                            && p.X < key.Data.Planet.Width
                            && p.Y <= key.Data.Planet.Height).ToList();
            if (newPoints.Count > 1 && newPoints[0] != newPoints[1])
            {
                graph.SetEdgeValue(keep, nextKeep, new LineSegment(newPoints[0], newPoints[1]));
            }
        }
        WrapVoronoiGraph<MapPolygon, LineSegment>(
            graph, keepMergePolys, discardMergePolys, 
            new MapPolygonEdgeConverter(key)
        );
        var check = new List<MapPolygon>(keepMergePolys);
        var discardHash = discardMergePolys.ToHashSet();
        discardMergePolys.ForEach(discard =>
        {
            var ns = discard.Neighbors.Items(key.Data).ToList();
            for (var i = ns.Count - 1; i >= 0; i--)
            {
                var discardN = ns[i];
                check.Add(discardN);
                var border = discard.GetEdge(discardN, key.Data);
                discardN.RemoveNeighbor(discard, key);
                discard.RemoveNeighbor(discardN, key);
                key.Data.RemoveEntity(border.Id, key);
            }
            key.Data.RemoveEntity(discard.Id, key);
        });
        check.ForEach(n =>
        {
            var badNs = n.Neighbors.Items(key.Data).Intersect(discardHash).ToList();
            foreach (var badN in badNs)
            {
                n.RemoveNeighbor(badN, key);
            }
        });
    }
    public static Graph<TNode, TEdge> GenerateVoronoiGraph<TNode, TEdge>
    (List<TNode> elements, Func<TNode, Vector2> posFunc,
        Func<Vector2, Vector2, TNode, TNode, TEdge> getEdgeFunc, Vector2 bounds) where TNode : class
    {
        var graph = new Graph<TNode, TEdge>();
        elements.ForEach(e => graph.AddNode(e));
        var iPoints = elements.Select(posFunc).Select(v => v.GetIPoint()).ToArray();
        var d = new Delaunator(iPoints);
        var voronoiCells = d.GetVoronoiCells().ToList();

        var pointElements = new Dictionary<IPoint, TNode>();
        var elementCells = new Dictionary<TNode, IVoronoiCell>();
        var cellElements = new Dictionary<IVoronoiCell, TNode>();
        for (var i = 0; i < elements.Count; i++)
        {
            elementCells.Add(elements[i], voronoiCells[i]);
            cellElements.Add(voronoiCells[i], elements[i]);
            pointElements.Add(iPoints[i], elements[i]);
        }

        var edges = new ConcurrentDictionary<Edge<TNode>, TEdge>();
        
        void makeGraphEdgeFromTriEdge(IEdge edge)
        {
            var tri = Mathf.FloorToInt(edge.Index / 3);
            var circum = d.GetTriangleCircumcenter(tri);
            var p1 = edge.P;
            var p2 = edge.Q;
            var el1 = pointElements[p1];
            var el2 = pointElements[p2];
            if(edges.ContainsKey(new Edge<TNode>(el1, el2, e => e.GetHashCode())))
            {
                throw new Exception();
            }
            if (d.Halfedges[edge.Index] != -1)
            {
                var oppEdgeIndex = d.Halfedges[edge.Index];
                var oppTri = Mathf.FloorToInt(oppEdgeIndex / 3);
                var oppCircum = d.GetTriangleCircumcenter(oppTri);
                var p = circum.GetIntV2();
                var oP = oppCircum.GetIntV2();

                if (p.Y >= bounds.Y) p.Y = bounds.Y;
                if (oP.Y >= bounds.Y) oP.Y = bounds.Y;
                if (p.X >= bounds.X) p.X = bounds.X;
                if (oP.X >= bounds.X) oP.X = bounds.X;
                if (p == oP)
                {
                    return;
                }
                var tEdge = getEdgeFunc(p, oP, el1, el2);
                edges.TryAdd(new Edge<TNode>(el1, el2, e => e.GetHashCode()), tEdge);
            }
            else
            {
                var secondPoint = ((edge.P.GetIntV2() + edge.Q.GetIntV2()) / 2f).Intify();
                var tEdge = getEdgeFunc(circum.GetIntV2(), secondPoint, el1, el2);
                edges.TryAdd(new Edge<TNode>(el1, el2, e => e.GetHashCode()), tEdge);
            }
        }

        Parallel.ForEach(d.GetEdges(), makeGraphEdgeFromTriEdge);
        
        foreach (var kvp in edges)
        {
            graph.AddEdge(kvp.Key.T1, kvp.Key.T2, kvp.Value);
        }
        
        return graph;
    }

    private class MapPolygonEdgeConverter : EdgeConverter<MapPolygon, LineSegment>
    {
        public MapPolygonEdgeConverter(GenWriteKey key)
            : base((discard, discardNeighbor, keep, oldEdge) =>
                {
                    discardNeighbor.RemoveNeighbor(discard, key);
                    discard.RemoveNeighbor(discardNeighbor, key);
                    return oldEdge;
                }
            )
        {
            
        }
    }
    private class EdgeConverter<TNode, TEdge>
    {
        private Func<TNode, TNode, TNode, TEdge, TEdge> _convertEdge;

        public EdgeConverter(Func<TNode, TNode, TNode, TEdge, TEdge> convertEdge)
        {
            _convertEdge = convertEdge;
        }

        public TEdge Convert(TNode discard, TNode discardNeighbor, TNode keep, TEdge oldEdge)
        {
            return _convertEdge(discard, discardNeighbor, keep, oldEdge);
        }
    }
    private static void WrapVoronoiGraph<TNode, TEdge>(Graph<TNode, TEdge> graph, 
        List<TNode> wrapKeep, List<TNode> wrapDiscard,
        EdgeConverter<TNode, TEdge> edgeConverter)
    {
        for (var i = 0; i < wrapKeep.Count; i++)
        {
            var keep = wrapKeep[i];
            var discard = wrapDiscard[i];
            var discardNeighbors = graph.GetNeighbors(discard);
            for (var j = 0; j < discardNeighbors.Count; j++)
            {
                var discardNeighbor = discardNeighbors.ElementAt(j);
                var oldEdge = graph.GetEdge(discard, discardNeighbor);
                var newEdge = edgeConverter.Convert(discard, discardNeighbor, keep, oldEdge);
                graph.AddEdge(keep, discardNeighbor, newEdge);
            }
        }
        
        wrapDiscard.ForEach(discard =>
        {
            graph.Remove(discard);
        });
    }
    
    
    
    public static Graph<TNode, TEdge> GenerateDelaunayGraph<TNode, TEdge>
        (List<TNode> elements, Func<TNode, Vector2> posFunc,
            Func<TNode,TNode,TEdge> getEdgeFunc)
    {
        var graph = new Graph<TNode, TEdge>();
        var poses = new List<Vector2>();
        for (int i = 0; i < elements.Count; i++)
        {
            poses.Add(posFunc(elements[i]));
            var node = new GraphNode<TNode, TEdge>(elements[i]);
            graph.AddNode(node);
        }
        var tris = Triangulator.TriangulatePoints(poses);
        for (int i = 0; i < tris.Count; i++)
        {
            var t = tris[i];
            var elementA = elements[poses.IndexOf(t.A)];
            var a = graph[elementA];
            
            var elementB = elements[poses.IndexOf(t.B)];
            var b = graph[elementB];
            
            var elementC = elements[poses.IndexOf(t.C)];
            var c = graph[elementC];

            graph.AddUndirectedEdge(a, b, getEdgeFunc(a.Element, b.Element));
            graph.AddUndirectedEdge(a, c, getEdgeFunc(a.Element, c.Element));
            graph.AddUndirectedEdge(c, b, getEdgeFunc(c.Element,b.Element));
        }

        return graph;
    }
   
    
}
