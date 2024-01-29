using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using GeometRi;


public static class TriangleExt 
{
    public static float GetDistFromEdge(this Triangle t, Vector2 point)
    {
        if (t.ContainsPoint(point)) return 0f;
        var close1 = point.GetClosestPointOnLineSegment(t.A, t.B);
        var dist1 = point.DistanceTo(close1);
        var close2 = point.GetClosestPointOnLineSegment(t.A, t.C);
        var dist2 = point.DistanceTo(close2);
        var close3 = point.GetClosestPointOnLineSegment(t.C, t.B);
        var dist3 = point.DistanceTo(close3);
        var res = Mathf.Min(dist1, dist2);
        res = Mathf.Min(res, dist3);
        return res;
    }
    public static Triangle GetInscribed(this Triangle t, float shrinkFactor)
    {
        var centroid = t.GetCentroid();
        return new Triangle(centroid + (t.A - centroid) * shrinkFactor,
            centroid + (t.B - centroid) * shrinkFactor,
            centroid + (t.C - centroid) * shrinkFactor);
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
    public static float GetMinAltitude(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        return Mathf.Min(p0.DistToLine(p1, p2), Mathf.Min(p1.DistToLine(p0, p2), p2.DistToLine(p0, p1)));
    }

    public static float GetApproxArea(this Triangle t)
    {
        return GetApproxArea(t.A, t.B, t.C);
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
    public static float GetApproxArea(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        var l0 = p0.DistanceTo(p1);
        var l1 = p1.DistanceTo(p2);
        var l2 = p2.DistanceTo(p0);
        var semiPerim = (l0 + l1 + l2) / 2f;
        var perimScore = semiPerim * (semiPerim - l0) * (semiPerim - l1) * (semiPerim - l2);
        if (perimScore < 0f)
        {
            return 0f;
        }
        var area = Mathf.Sqrt(semiPerim * (semiPerim - l0) * (semiPerim - l1) * (semiPerim - l2) );
        if (float.IsNaN(area)) throw new Exception($"bad tri area {p0} {p1} {p2} semi perims {l0} {l1} {l2}");
        return area;
    }
    public static bool ContainsPoint(this Triangle tri, Vector2 p)
    {
        return Geometry2D.IsPointInPolygon(p, new Vector2[] { tri.A, tri.B, tri.C });
    }
}