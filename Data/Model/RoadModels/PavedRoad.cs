using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PavedRoad : RoadModel
{
    public PavedRoad() 
        : base(nameof(PavedRoad), 6, Colors.Black.Lightened(.2f))
    {
    }

    public override void Draw(MeshBuilder mb, Vector2 from, Vector2 to, float width)
    {
        mb.AddLine(from, to, Color, width);
        mb.AddDashedLine(from, to, Colors.White, width / 4f, width / 2f, width / 3f);
    }
}
