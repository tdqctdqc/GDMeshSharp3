using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;


public class LineSegment : ISegment<Vector2>
{
    public Vector2 From { get; set; }
    public Vector2 To { get; set; }

    public Vector2 Mid() => (From + To) / 2f;
    public LineSegment(Vector2 from, Vector2 to)
    {
        From = from;
        To = to;
    }

    public void Flip()
    {
        var temp = From;
        From = To;
        To = temp;
    }
    public LineSegment Reverse()
    {
        return new LineSegment(To, From);
    }

    public LineSegment ChangeOrigin(Vector2 oldOrigin, Vector2 newOrigin)
    {
        return new LineSegment(From + oldOrigin - newOrigin, To + oldOrigin - newOrigin);
    }

    public LineSegment Translate(Vector2 offset)
    {
        return new LineSegment(From + offset, To + offset);
    }

    public LineSegment Rotated(float rads, float shrink = 1f)
    {
        var mid = (From + To) / 2f;
        var axis = (To - From).Orthogonal();
        return new LineSegment(
            mid - axis * .5f * shrink, mid + axis * .5f * shrink
        );
    }
    public void Clamp(float mapWidth)
    {
        if (Mid().X > mapWidth / 2f)
        {
            From += Vector2.Left * mapWidth;
            To += Vector2.Left * mapWidth;
        }

        if (Mid().X < -mapWidth / 2f)
        {
            From += Vector2.Right * mapWidth;
            To += Vector2.Right * mapWidth;
        }
    }

    public float DistanceTo(Vector2 point)
    {
        return point.DistToLine(From, To);
    }

    public float Length()
    {
        return From.DistanceTo(To);
    }

    public bool ContainsPoint(Vector2 p)
    {
        return (p - From).Normalized() == (To - p).Normalized();
    }
    public bool ContainsVertex(Vector2 p)
    {
        return p == From || p == To;
    }
    public bool LeftOf(Vector2 point)
    {
        return (To.X - From.X)*(point.Y - From.Y) - (To.Y - From.Y)*(point.X - From.X) > 0;
    }

    public Vector2 GetNormalizedAxis()
    {
        return (To - From).Normalized();
    }
    public Vector2 GetNormalizedPerpendicular()
    {
        return (To - From).Orthogonal().Normalized();
    }
    public override string ToString()
    {
        return $"[from {From} to {To}] \b";
    }
    ISegment<Vector2> ISegment<Vector2>.ReverseGeneric() => Reverse();
    bool ISegment<Vector2>.PointsTo(ISegment<Vector2> s)
    {
        return To == s.From;
    }

    bool ISegment<Vector2>.ComesFrom(ISegment<Vector2> s)
    {
        if (s is ISegment<Vector2> t == false) return false;
        return From == t.To;
    }

    public LineSegment Copy()
    {
        return new LineSegment(From, To);
    }
}

