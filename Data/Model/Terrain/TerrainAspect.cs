using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class TerrainAspect : IModel
{
    public abstract string Name { get; protected set; }
    public abstract int Id { get; protected set; }
    public abstract Color Color { get; protected set; }
    public byte Marker { get; private set; }
    public TerrainAspect()
    {
    }

    public void SetMarker(byte marker)
    {
        Marker = marker;
    }
}