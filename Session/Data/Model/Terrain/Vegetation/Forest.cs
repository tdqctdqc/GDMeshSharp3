
using System.Collections.Generic;
using Godot;

public class Forest : Vegetation, IDecaledTerrain
{
    public Forest(LandformList lfs) : base(new HashSet<Landform>{lfs.Hill, lfs.Plain}, 
        .4f, .5f, Colors.LimeGreen.Darkened(.4f), "Forest")
    {
        
    }

    public void GetDecal(MeshBuilder mb, PolyTri pt, Vector2 offset)
    {
        
    }
    
    
    Mesh IDecaledTerrain.GetDecal()
    {
        var size = 3f;
        var offset = Vector2.Down * size / 2f;
        return MeshGenerator.GetArrayMesh(new Vector2[]{
            Vector2.Left * size + offset,
            Vector2.Right * size + offset,
            Vector2.Up * size * 2f + offset
        });
    }
    float IDecaledTerrain.DecalSpacing => 5f;
}
