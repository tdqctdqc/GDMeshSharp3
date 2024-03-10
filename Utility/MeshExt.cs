
using Godot;

public static class MeshExt
{
    public static MeshInstance2D GetQuadMeshInstance(Vector2 size)
    {
        var q = new QuadMesh();
        q.Size = size;
        var mesh = new MeshInstance2D();
        mesh.Mesh = q;
        return mesh;
    }
    public static QuadMesh GetQuadMesh(Vector2 size)
    {
        var q = new QuadMesh();
        q.Size = size;
        return q;
    }
}