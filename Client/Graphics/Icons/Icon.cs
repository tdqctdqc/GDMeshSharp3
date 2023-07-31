
using System;
using Godot;

public partial class Icon : MeshTexture
{
    public string Name { get; private set; }
    public Vector2 Dimension { get; private set; }
    public enum AspectRatio
    {
        _1x1, _2x3, _1x2
    }
    public static Icon Create(string textureName, AspectRatio ratio, float bigDimSize)
    {
        var i = new Icon();
        QuadMesh q;
        if (ratio == AspectRatio._1x1)
        {
            q = new QuadMesh();
            q.Size = Vector2.One * bigDimSize;
        }
        else if (ratio == AspectRatio._1x2)
        {
            q = q = new QuadMesh();
            q.Size = new Vector2(bigDimSize / 2f, bigDimSize);
        }
        else if (ratio == AspectRatio._2x3)
        {
            q = q = new QuadMesh();
            q.Size = new Vector2(bigDimSize * 2f / 3f, bigDimSize);
        }
        else throw new Exception();
        
        i.Mesh = q;
        i.Dimension = q.Size;
        i.Name = textureName;
        i.BaseTexture = TextureManager.Textures[textureName];
        return i;
    }
    public Icon()
    {
        
    }
    private static QuadMesh MakeMesh(Vector2 size)
    {
        var m = new QuadMesh();
        m.Size = size;
        return m;
    }

    public MeshInstance2D GetMeshInstance()
    {
        var mi = new MeshInstance2D();
        mi.Scale = new Vector2(1f, -1f);
        mi.Mesh = Mesh;
        mi.Texture = BaseTexture;
        return mi;
    }

    public TextureRect GetTextureRect(Vector2 dim)
    {
        var rect = new TextureRect();
        rect.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
        rect.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
        rect.Size = dim;
        rect.CustomMinimumSize = dim;
        rect.Texture = BaseTexture;
        rect.FlipH = true;
        return rect;
    }
}
