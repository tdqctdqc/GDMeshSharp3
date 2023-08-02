using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Urban : Landform, IDecaledTerrain
{
    
    public Urban() 
        : base("Urban", 1000f, 0f, Colors.Purple, false)
    {
        
    }

    
    
    Mesh IDecaledTerrain.GetDecal()
    {
        var spacing = ((IDecaledTerrain) this).DecalSpacing;
        var offset = Vector2.Down * spacing / 2f;
        var size = 5f;
        return MeshGenerator.GetArrayMesh(new Vector2[]{
            Vector2.Left * size + offset,
            Vector2.Right * size + offset,
            Vector2.Up * size * 5f + Vector2.Left * size + offset,
            Vector2.Right * size + offset,
            Vector2.Up * size * 5f + Vector2.Left * size + offset,
            Vector2.Up * size * 5f + Vector2.Right * size + offset
        });
    }
    float IDecaledTerrain.DecalSpacing => 10f;
}