using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RoadModel : IModel
{
    public string Name { get; private set; }
    IReadOnlyList<IModelAttribute> IModel.AttributeList => Attributes;
    public AttributeHolder<IModelAttribute> Attributes { get; }
    public Color Color { get; private set; }
    public int Speed { get; private set; }
    public int Id { get; private set; }


    public RoadModel(string name, int speed, Color color)
    {
        Color = color;
        Name = name;
        Speed = speed;
        Attributes = new AttributeHolder<IModelAttribute>();
    }

    public virtual void Draw(MeshBuilder mb, Vector2 from, Vector2 to, float width)
    {
        mb.AddLine(from, to, Color, width);
    }

}
