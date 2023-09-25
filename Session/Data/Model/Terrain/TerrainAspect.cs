using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class TerrainAspect : IModel
{
    public abstract string Name { get; protected set; }
    public AttributeHolder<IModelAttribute> Attributes { get; }
    public abstract int Id { get; protected set; }
    public abstract Color Color { get; protected set; }
    public byte Marker { get; private set; }
    IReadOnlyList<IModelAttribute> IModel.AttributeList => Attributes;
    public TerrainAspect()
    {
        Attributes = new AttributeHolder<IModelAttribute>();
    }

    public void SetMarker(byte marker)
    {
        Marker = marker;
    }
}