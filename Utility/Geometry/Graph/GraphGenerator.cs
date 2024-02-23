using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DelaunatorSharp;

public static class GraphGenerator
{

    
    public static Graph<TNode, TEdge> GenerateVoronoiGraph<TNode, TEdge>
    (List<TNode> elements, Func<TNode, Vector2> posFunc,
        Func<Vector2, Vector2, TNode, TNode, TEdge> getEdgeFunc, 
        Vector2 bounds) 
        // where TNode : class
    {
        var graph = new Graph<TNode, TEdge>();
        elements.ForEach(e => graph.AddNode(e));
        var iPoints = elements.Select(posFunc).Select(v => v.GetIPoint()).ToArray();
        var d = new Delaunator(iPoints);
        
        var pointElements = new Dictionary<IPoint, TNode>();
        for (var i = 0; i < elements.Count; i++)
        {
            pointElements.Add(iPoints[i], elements[i]);
        }

        var edges = new ConcurrentDictionary<Edge<TNode>, TEdge>();
        Parallel.ForEach(d.GetEdges(), makeGraphEdgeFromTriEdge);
        
        foreach (var kvp in edges)
        {
            graph.AddEdge(kvp.Key.T1, kvp.Key.T2, kvp.Value);
        }
        
        
        
        
        return graph;
        
        
        
        
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

                if (bounds != Vector2.Inf)
                {
                    if (p.Y >= bounds.Y) p.Y = bounds.Y;
                    if (oP.Y >= bounds.Y) oP.Y = bounds.Y;
                    if (p.X >= bounds.X) p.X = bounds.X;
                    if (oP.X >= bounds.X) oP.X = bounds.X;
                }
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
