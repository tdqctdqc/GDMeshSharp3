using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using Godot;

public static class DelaunayExt
{
    public static (Vector2 p1, Vector2 p2,
        (Vector2, Vector2))[] 
        GetVoronoiGraphNew(this Delaunator delaunay)
    {
        return delaunay.GetEdges()
            .AsParallel()
            .Select(getInfo)
            .Where(i => i.p1 != i.p2)
            .ToArray();
        
        (Vector2 p1, Vector2 p2, (Vector2, Vector2) e) 
            getInfo(IEdge e)
        {
            var e1 = e.Index;
            var p1 = delaunay.Points[delaunay.Triangles[e1]];
            var e2 = delaunay.Halfedges[e1];
            if (e2 == -1)
            {
                return default;
            }
            var p2 = delaunay.Points[delaunay.Triangles[e2]];
            var v1 = p1.GetV2();
            var v2 = p2.GetV2();
            
            var t1 = Mathf.FloorToInt(e1 / 3);
            var c1 = delaunay.GetTriangleCircumcenter(t1).GetV2();
            var t2 = Mathf.FloorToInt(e2 / 3);
            var c2 = delaunay.GetTriangleCircumcenter(t2).GetV2();
            return (v1, v2, (c1, c2));
        }
    }
}