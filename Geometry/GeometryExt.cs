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

    public static bool PointIsOnLineSegment(this Vector2 point, Vector2 seg1, Vector2 seg2)
    {
        var axis = (seg1 - seg2).Normalized();
        var ray = (seg1 - point).Normalized();
        if (axis != ray && axis != -ray) return false;
        return ((seg1.X <= point.X && point.X <= seg2.X) || (seg2.X <= point.X && point.X <= seg1.X))
               &&
               ((seg1.Y <= point.Y && point.Y <= seg2.Y) || (seg2.Y <= point.Y && point.Y <= seg1.Y));
    }
}
