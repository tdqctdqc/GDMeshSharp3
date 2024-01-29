using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ColorTri : Triangle
{
    public Color Color { get; private set; }

    public ColorTri(Color color, Vector2 a, Vector2 b, Vector2 c) 
        : base(a, b, c)
    {
        Color = color;
    }
}
