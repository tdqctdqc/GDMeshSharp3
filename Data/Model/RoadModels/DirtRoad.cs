using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class DirtRoad : RoadModel
{
    public DirtRoad() 
    {
    }

    public override void Draw(MeshBuilder mb, Vector2 from, Vector2 to, float width)
    {
        mb.AddLine(from, to, Colors.SaddleBrown, width);
        mb.AddParallelLines(from, to, Colors.SaddleBrown.Darkened(.5f),
            width / 6f, width / 4f);
    }
}
