
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class Clockwise
{
    //SOURCES OF TRUTH
    public static bool IsCCW(Vector2 a, Vector2 b, Vector2 c)
    {
        var cross = (a - b).Cross(c - b);
        return (a - b).Cross(c - b) > 0f;
    }
    public static float GetCCWAngleTo(this Vector2 v, Vector2 to)
    {
        if (v == to) return 0f;
        return (2f * Mathf.Pi + v.AngleTo(to)) % (2f * Mathf.Pi);
    }
    private static void OrderByClockwiseDir<T>(List<T> elements, Vector2 center, 
        Func<T, Vector2> elPos, int dir)
    {
        var first = elPos(elements.First()) - center;
        Comparison<T> comp =  (i,j) => 
            dir * (elPos(j) - center).GetCWAngleTo(first)
            .CompareTo( (elPos(i) - center).GetCWAngleTo(first) );
        elements.Sort(comp);
    }
    
    
    //IMPLICIT 
    public static void OrderByClockwise<T>(this List<T> elements, 
            Vector2 center, 
            Func<T, Vector2> elPos)
    {
        OrderByClockwiseDir(elements, center, elPos, 1);
    }
    public static void OrderByCCW<T>(this List<T> elements, 
        Vector2 center, 
        Func<T, Vector2> elPos)
    {
        OrderByClockwiseDir(elements, center, elPos, -1);
    }
    public static bool IsClockwise(this LineSegment seg, Vector2 center)
    {
        return IsClockwise(seg.From, seg.To, center);
    }
    public static bool IsCCW(this LineSegment seg, Vector2 center)
    {
        return IsCCW(seg.From, seg.To, center);
    }
    public static bool IsClockwise(Vector2 a, Vector2 b, Vector2 c)
    {
        return IsCCW(a, b, c) == false;
    }
    
    public static float GetCCWAngle(this Vector2 v)
    {
        return v.GetCCWAngleTo(Vector2.Right);
    }
    public static float GetClockwiseAngle(this Vector2 v)
    {
        return 2f * Mathf.Pi - GetCCWAngle(v);
    }
    public static float GetCWAngleTo(this Vector2 v, Vector2 to)
    {
        return 2f * Mathf.Pi - GetCCWAngleTo(v, to);
    }
}
