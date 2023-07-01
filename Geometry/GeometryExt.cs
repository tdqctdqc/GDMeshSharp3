using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class GeometryExt
{
    

    public static float RadToDegrees(this float rad)
    {
        return (360f * rad / (Mathf.Pi * 2f));
    }
}
