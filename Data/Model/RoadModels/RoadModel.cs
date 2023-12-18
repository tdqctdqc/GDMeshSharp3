using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RoadModel : IModel
{
    public string Name { get; private set; }
    public Color Color { get; private set; }
    public float SpeedMult { get; private set; }
    public int Id { get; private set; }


    public RoadModel(string name, float speedMult, Color color)
    {
        Color = color;
        Name = name;
        SpeedMult = speedMult;
    }

    public virtual void Draw(MeshBuilder mb, Vector2 from, Vector2 to, float width)
    {
        mb.AddLine(from, to, Color, width);
    }

}
