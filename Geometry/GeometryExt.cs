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
}
