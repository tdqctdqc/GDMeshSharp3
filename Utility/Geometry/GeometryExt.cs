using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class GeometryExt
{
    public static bool InBox(this Vector2 point, Vector2 boxPos, Vector2 boxDim)
    {
        return point.X >= boxPos.X && point.X <= boxPos.X + boxDim.X
         && point.Y >= boxPos.Y && point.Y <= boxPos.Y + boxDim.Y;
    }

    public static float RadToDegrees(this float rad)
    {
        return (360f * rad / (Mathf.Pi * 2f));
    }
    public static float DegreesToRad(this float degrees)
    {
        return Mathf.Pi * 2f * degrees / 360f;
    }
    public static bool PointIsOnLineSegment(this Vector2 point, Vector2 seg1, Vector2 seg2)
    {
        var axis = (seg1 - seg2).Normalized();
        var ray = (seg1 - point).Normalized();
        if (axis != ray && axis != -ray) return false;
        return ((seg1.X <= point.X && point.X <= seg2.X) || (seg2.X <= point.X && point.X <= seg1.X))
               &&
               ((seg1.Y <= point.Y && point.Y <= seg2.Y) || (seg2.Y <= point.Y && point.Y <= seg1.Y));
    }

    public static Vector2[] UnifyPolygons(this Vector2[] poly1, Vector2[] poly2)
    {
        var unions = Geometry2D.MergePolygons(poly1, poly2);
        if (unions.Count != 1) throw new Exception();
        return unions[0];
    }
    public static bool TryUnifyPolygons(this Vector2[] poly1, 
        Vector2[] poly2, out Vector2[] newBounds)
    {
        newBounds = null;
        var unions = Geometry2D.MergePolygons(poly1, poly2);
        if (unions.Count != 1) return false;
        newBounds = unions[0];
        return true;
    }
    public static Vector2[] UnifyPolygons(params Vector2[][] polys)
    {
        Vector2[] union = polys[0];
        for (var i = 1; i < polys.Length; i++)
        {
            var intersects = Geometry2D.IntersectPolygons(union,
                polys[i]);
            if (intersects.Count != 1) return null;
            union = intersects[0];
        }

        return union;
    }
}
