using Godot;

public class Peak : Landform, IDecaledTerrain
{
    public Peak() 
        : base("Peak", .8f, 0f, Colors.Snow.Darkened(.25f), false)
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
}
