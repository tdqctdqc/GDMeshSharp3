
using Godot;

public class Mountain : Landform, IDecaledTerrain
{
    public Mountain() 
        : base("Mountain", .6f, 0f, Colors.DimGray, false)
    {
    }

    Mesh IDecaledTerrain.GetDecal()
    {
        var spacing = ((IDecaledTerrain) this).DecalSpacing;
        var offset = Vector2.Down * spacing / 2f;
        return MeshGenerator.GetArrayMesh(new Vector2[]{Vector2.Left * spacing + offset,
            Vector2.Right * spacing + offset,
            Vector2.Up * spacing + offset});
    }
    float IDecaledTerrain.DecalSpacing => 30f;
    Color IDecaledTerrain.GetDecalColor(PolyTri pt) => Color.Darkened(.25f);
}
