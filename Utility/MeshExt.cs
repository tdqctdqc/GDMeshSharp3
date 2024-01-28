
using Godot;

public static class MeshExt
{
    public static QuadMesh GetQuadMesh(Vector2 size)
    {
        var q = new QuadMesh();
        q.Size = size;
        return q;
    }
}