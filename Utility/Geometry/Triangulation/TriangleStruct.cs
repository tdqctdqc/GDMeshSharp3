using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public struct TriangleStruct
{
    public Vector2 A { get; set; }
    public Vector2 B { get; set; }
    public Vector2 C { get; set; }

    public TriangleStruct(Vector2 a, Vector2 b, Vector2 c)
    {
        A = a;
        B = b;
        C = c;
    }
}
