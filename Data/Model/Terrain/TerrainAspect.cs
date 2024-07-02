using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class TerrainAspect : IModel
{
    public string Name { get; protected set; }
    public int Id { get; protected set; }
    public Color Color { get; protected set; }
    public TerrainAspect()
    {
    }

    public MeshTexture GetColorTexture(float size)
    {
        var mesh = MeshGenerator.GetSquareMesh(size, Color);
        var texture = new MeshTexture();
        texture.Mesh = mesh;
        return texture;
    }
}