using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BadPointsException : DisplayableException
{
    public List<Vector2> Points { get; private set; }
    public BadPointsException(MapPolygon poly, IReadOnlyList<Vector2> points, GenData data)
    {
        Points = points.ToList();
    }
    public override Node2D GetGraphic()
    {
        var mb = new MeshBuilder();
        mb.AddPointMarkers(Points, 20f, Colors.White);
        return mb.GetMeshInstance();
    }

    public override Control GetUi()
    {
        return new Control();
    }
}
