using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public struct LineSegStruct
{
    public Vector2 A { get; private set; }
    public Vector2 B { get; private set; }

    public LineSegStruct(LineSegment ls)
    {
        A = Vector2.Zero;
        B = Vector2.Zero;
        Assign(ls.From, ls.To);
    }
    public LineSegStruct(Vector2 a, Vector2 b)
    {
        A = Vector2.Zero;
        B = Vector2.Zero;
        Assign(a, b);
    }
    private void Assign(Vector2 a, Vector2 b)
    {
        if (a.X != b.X)
        {
            if (a.X < b.X)
            {
                A = a;
                B = b;
            }
            else
            {
                B = a;
                A = b;
            }
        }
        else if (a.Y != b.Y)
        {
            if (a.Y < b.Y)
            {
                A = a;
                B = b;
            }
            else
            {
                B = a;
                A = b;
            }
        }
        else
        {
            A = a;
            B = b;
        }
    }
}
public struct LineSegStructNoFlip
{
    public Vector2 A { get; private set; }
    public Vector2 B { get; private set; }

    public LineSegStructNoFlip(LineSegment ls)
    {
        A = ls.From;
        B = ls.To;
    }
    public LineSegStructNoFlip(Vector2 a, Vector2 b)
    {
        A = a;
        B = b;
    }
    
}
