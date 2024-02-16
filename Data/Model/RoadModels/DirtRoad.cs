using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class DirtRoad : RoadModel
{
    public DirtRoad() 
        : base(nameof(DirtRoad), 
            .25f, 
            false, 0f,
            Colors.SaddleBrown)
    {
    }

    public override void Draw(MeshBuilder mb, Vector2 from, Vector2 to, float width)
    {
        base.Draw(mb, from, to, width);
        mb.AddParallelLines(from, to, Colors.SaddleBrown.Darkened(.5f),
            width / 6f, width / 4f);
    }
}
