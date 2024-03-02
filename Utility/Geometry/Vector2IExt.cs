
using System.Collections;
using System.Collections.Generic;
using DelaunatorSharp;
using Godot;

public static class Vector2IExt
{
    public static IPoint GetIPoint(this Vector2I v)
    {
        return new Triangulator.DelaunatorPoint(v);
    }

    public static IEnumerable<int> Yield(this Vector2I v)
    {
        yield return v.X;
        yield return v.Y;
    }
    public static Vector2I ClampPosition(this Vector2I pos, Vector2I dim)
    {
        var width = dim.X;
        var height = dim.Y;
        if (pos.Y < 0 ) pos.Y = 0;
        if (pos.Y > height) pos.Y = height;
        while (pos.X < 0) pos += Vector2I.Right * width;
        while (pos.X > width) pos += Vector2I.Left * width;
        return pos;
    }
    public static Vector2I ClampPosition(this Vector2 pos, Vector2I dim)
    {
        var width = dim.X;
        var height = dim.Y;
        if (pos.Y < 0 ) pos.Y = 0;
        if (pos.Y > height) pos.Y = height;
        while (pos.X < 0) pos += Vector2I.Right * width;
        while (pos.X > width) pos += Vector2I.Left * width;
        return (Vector2I)pos;
    }
}