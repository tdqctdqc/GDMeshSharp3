using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class TerrainAspect : IModel
{
    public string Name { get; protected set; }
    public int Id { get; protected set; }
    public Color Color { get; protected set; }
    public byte Marker { get; private set; }
    public TerrainAspect()
    {
    }

    public void SetMarker(byte marker)
    {
        Marker = marker;
    }
}