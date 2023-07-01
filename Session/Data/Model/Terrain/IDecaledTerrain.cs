using Godot;

public interface IDecaledTerrain
{
    Mesh GetDecal();
    float DecalSpacing { get; }
    Color GetDecalColor(PolyTri pt);
}
