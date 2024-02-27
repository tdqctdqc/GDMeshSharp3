using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DelaunatorSharp;
using Godot;

namespace VoronoiSandbox;

public static class DelaunayExt
{
    public static int NextHalfEdge(this Delaunator d, int e)
    {
        return (e % 3 == 2) ? e - 2 : e + 1;
    }
    public static int PrevHalfEdge(this Delaunator d, int e)
    {
        return (e % 3 == 0) ? e + 2 : e - 1; 
    }

    public static int TriangleOfEdge(int e)
    {
        return Mathf.FloorToInt(e / 3);
    }
    public static (Vector2I p1, Vector2I p2,
        (Vector2I, Vector2I))[] 
        GetVoronoiGraphNew(this Delaunator delaunay, 
            PreCellResult preCellResult,
            Vector2I dim, GenWriteKey key)
    {
        var splitBag = new 
            ConcurrentDictionary<Vector2I, 
                (Vector2I a1, Vector2I a2, Vector2I c1, Vector2I c2)>();
        preCellResult.SplitBag = splitBag;
        var res = delaunay.GetEdges()
            .AsParallel()
            .Select(getInfo)
            .Where(i => i.p1 != i.p2)
            .ToArray();
        HandleSplitPointAdjacentEdges(dim, res, splitBag, key);
        
        return res;
        
        (Vector2I p1, Vector2I p2, (Vector2I, Vector2I) e) 
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
            
            var v1 = (Vector2I)p1.GetV2().Intify();
            var v2 = (Vector2I)p2.GetV2().Intify();
            
            var t1 = Mathf.FloorToInt(e1 / 3);
            var c1 = delaunay.GetTriangleCircumcenter(t1).GetV2().Intify();
            var t2 = Mathf.FloorToInt(e2 / 3);
            var c2 = delaunay.GetTriangleCircumcenter(t2).GetV2().Intify();
            
            var c1I = (Vector2I)c1.Intify();
            var c2I = (Vector2I)c2.Intify();
            
            if (c1 == c2)
            {
                var newPs = SplitPoint(c1I, v1, v2, dim, key);
                splitBag.TryAdd(c1I, (v1, v2, newPs.Item1, newPs.Item2));
                return (v1, v2, newPs);
            }
            return (v1, v2, (c1I, c2I));
        }
    }

    private static void HandleSplitPointAdjacentEdges(Vector2I dim,
        (Vector2I p1, Vector2I p2, (Vector2I, Vector2I) e)[] res,
        ConcurrentDictionary<Vector2I, (Vector2I a1, Vector2I a2, Vector2I c1, Vector2I c2)> splitPoint,
        GenWriteKey key)
    {
        Parallel.ForEach(Enumerable.Range(0, res.Length - 1), i =>
        {
            var v = res[i];
            var (p1, p2, (c1, c2)) = v;
            res[i] = (p1, p2, (replace(c1), replace(c2)));
        
            Vector2I replace(Vector2I c)
            {
                if (splitPoint.ContainsKey(c) == false) return c;
                var split = splitPoint[c];
                var (a1, a2, n1, n2) = split;
                Vector2I shared;
                var shares1 = a1 == p1 || a2 == p1;
                var shares2 = a1 == p2 || a2 == p2;
                if (shares1 && shares2) return c;
                
                if (shares1) shared = p2;
                else if (shares2) shared = p1;
                else throw new Exception();
                
                return shared.Offset(n1, key.Data).Length() < shared.Offset(n2, key.Data).Length()
                    ? n1
                    : n2;
            }
        });
    }

    private static (Vector2I, Vector2I) SplitPoint(Vector2I point,
        Vector2I cellPoint1, Vector2I cellPoint2, Vector2I dim,
        GenWriteKey key)
    {
        var axis = (Vector2)cellPoint1.Offset(cellPoint2, key.Data);

        var mid = cellPoint1 + axis / 2;
        
        var perp = axis.Orthogonal().Normalized() * 5f;
        var arm1 = perp;
        var arm2 = -perp;

        var newPoint1 = (Vector2I)(mid + arm1);
        var newPoint2 = (Vector2I)(mid + arm2);

        if (newPoint1 == newPoint2)
        {
            throw new Exception();
        }
        
        return (newPoint1, newPoint2);
    }
}