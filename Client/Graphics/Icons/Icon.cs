
using System;
using Godot;

public class Icon
{
    public string Name { get; private set; }
    public Texture2D Texture { get; private set; }
    public Vector2I AspectRatio { get; private set; }
    public static Icon Create(string textureName, Vector2I ratio)
    {
        var i = new Icon();
        i.Name = textureName;
        i.Texture = TextureManager.Textures[textureName];
        i.AspectRatio = ratio;
        return i;
    }
    private Icon()
    {
        
    }

    public Vector2 GetDim(float height)
    {
        return (Vector2)AspectRatio * height / AspectRatio.Y;
    }
    public TextureRect GetTextureRect(float height)
    {
        var rect = new TextureRect();
        rect.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
        rect.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
        var size = GetDim(height);
        rect.Size = size;
        rect.CustomMinimumSize = size;
        rect.Texture = Texture;
        return rect;
    }
    public MeshInstance2D GetMeshInstance(float height)
    {
        var mi = new MeshInstance2D();
        var size = GetDim(height);
        var mesh = new QuadMesh();
        mesh.Size = size;
        mi.Texture = Texture;
        mi.Mesh = mesh;
        mi.Scale = new Vector2(1f, -1f);
        return mi;
    }
}
