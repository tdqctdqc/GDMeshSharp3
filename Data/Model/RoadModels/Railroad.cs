using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Railroad : RoadModel
{
    public Railroad() 
        : base(nameof(Railroad), 12, Colors.Black)
    {
    }

    public override void Draw(MeshBuilder mb, Vector2 from, Vector2 to, float width)
    {
        var railWidth = width / 5f;
        var woodWidth = width / 3f;
        var railDist = width / 2f;
        var crossBarDist = width / 2f;
        mb.AddSpacedCrossbars(from, to, Colors.Brown.Darkened(.3f), woodWidth, width, crossBarDist);
        mb.AddParallelLines(from, to, Colors.Gray, railWidth, railDist);
    }
}
