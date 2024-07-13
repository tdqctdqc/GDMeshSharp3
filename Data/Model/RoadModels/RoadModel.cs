using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class RoadModel : IModel
{
    public string Name { get; private set; }
    public float CostOverride { get; private set; }
    public bool UseSpeedOverride { get; private set; }
    public float SpeedOverride { get; private set; }
    public int Id { get; private set; }

    public RoadModel()
    {
    }

    public abstract void Draw(MeshBuilder mb, Vector2 from, Vector2 to, float width);

}
