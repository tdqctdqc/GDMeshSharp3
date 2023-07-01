using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using GeometRi;


public static class TriangleExt 
{
    public static List<Vector2> GetPoissonPointsInside(this Triangle t, float radius, float marginRatio = .5f)
    {
        var dim = t.GetDimensions();
        var shift = t.GetCentroid() - dim / 2f;
        return PointsGenerator.GeneratePoissonPoints(radius, dim, 30)
            .Where(p => t.ContainsPoint(p + shift))
            .Select(p => p + shift)
            .ToList();
    }

    public static bool SharesEdge(this Triangle t, Triangle r)
    {
        int pointsShared = 0;
        t.ForEachPoint(v =>
        {
            if (r.PointIsVertex(v)) pointsShared++;
        });
        if (pointsShared > 1) return true;
        return t.AnyPointPairs((p, q) =>
        {
            return r.AnyPointPairs((v, w) =>
            {
                return Vector2Ext.PointIsInLineSegment(p, v, w)
                    && Vector2Ext.PointIsInLineSegment(q, v, w);
            });
        });
    }
    public static List<LineSegment> GetSegments(this Triangle t)
    {
        var res = new List<LineSegment>();
        res.Add(new LineSegment(t.A, t.B));
        res.Add(new LineSegment(t.B, t.C));
        res.Add(new LineSegment(t.C, t.A));
        return res;
    }
    public static Triangle GetInscribed(this Triangle t, float shrinkFactor)
    {
        var centroid = t.GetCentroid();
        return new Triangle(centroid + (t.A - centroid) * shrinkFactor,
            centroid + (t.B - centroid) * shrinkFactor,
            centroid + (t.C - centroid) * shrinkFactor);
    }
    public static bool IsClockwise(this Triangle tri)
    {
        return Clockwise.IsCCW(tri.A, tri.B, tri.C);
    }
    public static List<Vector2> GetTriPoints(this List<Triangle> tris)
    {
        var res = new List<Vector2>();
        for (var i = 0; i < tris.Count; i++)
        {
            res.Add(tris[i].A);
            res.Add(tris[i].B);
            res.Add(tris[i].C);
        }
        return res;
    }
    public static float GetTriangleArea(Vector2 a, Vector2 b, Vector2 c)
    {
        return .5f * Mathf.Abs(a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y));
    }

    public static void AddTriPointsToCollection(this Triangle tri, ICollection<Vector2> col)
    {
        col.Add(tri.A);
        col.Add(tri.B);
        col.Add(tri.C);
    }
    
    public static float GetMinEdgeLength(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        var dist1 = p0.DistanceTo(p1);
        var dist2 = p0.DistanceTo(p2);
        var dist3 = p1.DistanceTo(p2);
        float min = Mathf.Min(dist1, dist2);
        return Mathf.Min(min, dist3);
    }

    public static float GetMinAltitude(this Triangle tri)
    {
        return GetMinAltitude(tri.A, tri.B, tri.C);
    }
    public static float GetMinAltitude(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        return Mathf.Min(p0.DistToLine(p1, p2), Mathf.Min(p1.DistToLine(p0, p2), p2.DistToLine(p0, p1)));
    }
    public static float GetMinAltitude(List<Vector2> points)
    {
        return GetMinAltitude(points[0], points[1], points[2]);
    }

    public static float GetArea(this Triangle t)
    {
        return GetArea(t.A, t.B, t.C);
    }

    public static List<Vector2>  GetRandomPointsInside(this Triangle t, int count, float minArcRatio, float maxArcRatio)
    {
        return Enumerable.Range(0, count).Select(i => GetRandomPointInside(t, minArcRatio, maxArcRatio)).ToList();
    }
    public static Vector2 GetRandomPointInside(this Triangle t, float minArcRatio, float maxArcRatio)
    {
        var arc1 = t.B - t.A;
        var arc2 = t.C - t.A;
        var totalArcRatio = Game.I.Random.RandfRange(minArcRatio, maxArcRatio);
        var arc1Ratio = Game.I.Random.RandfRange(0f, totalArcRatio);
        var arc2Ratio = totalArcRatio - arc1Ratio;
        return t.A + arc1 * arc1Ratio + arc2 * arc2Ratio;
    }

    

    public static float GetArea(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        var l0 = p0.DistanceTo(p1);
        var l1 = p1.DistanceTo(p2);
        var l2 = p2.DistanceTo(p0);
        var semiPerim = (l0 + l1 + l2) / 2f;
        return Mathf.Sqrt( semiPerim * (semiPerim - l0) * (semiPerim - l1) * (semiPerim - l2) );
    }

    

    public static bool IsDegenerate(this Triangle tri)
    {
        if ((tri.B - tri.A).Normalized() == (tri.C - tri.A).Normalized()) return true;
        return false;
    }
    public static bool IsDegenerate(Vector2 a, Vector2 b, Vector2 c)
    {
        if ((b - a).Normalized() == (c - a).Normalized()) return true;
        return false;
    }
    public static bool ContainsPoint(this Triangle tri, Vector2 p)
    {
        return ContainsPoint(tri.A, tri.B, tri.C, p);
    }
    public static bool ContainsPoint(Vector2 t1, Vector2 t2, Vector2 t3, Vector2 p)
    {
        float sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }
        var d1 = sign(p, t1, t2);
        var d2 = sign(p, t2, t3);
        var d3 = sign(p, t3, t1);

        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNeg && hasPos);
    }

    public static void CollectSegStructs(this Triangle tri, HashSet<LineSegStruct> col)
    {
        col.Add(new LineSegStruct(tri.A, tri.B));
        col.Add(new LineSegStruct(tri.C, tri.B));
        col.Add(new LineSegStruct(tri.A, tri.C));
    }
}