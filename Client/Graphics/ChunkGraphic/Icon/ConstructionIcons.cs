using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ConstructionIcons : MapChunkGraphicNode<Construction>
{
    private static Texture2D _underConstruction => TextureManager.Textures["UnderConstruction"];
    public ConstructionIcons(MapChunk chunk, Data data) 
        : base(nameof(ConstructionIcons), data, chunk)
    {        
    }
    private ConstructionIcons() : base()
    {
    }
    protected override Node2D MakeGraphic(Construction element, Data data)
    {
        var construction = element;
        var icon = construction.Model.Model(data).Icon.GetMeshInstance();
        var constrSignMesh = new MeshInstance2D();
        var mesh = new QuadMesh();
        mesh.Size = Vector2.One * 25f;
        constrSignMesh.Mesh = mesh;
        constrSignMesh.Texture = _underConstruction;
        icon.AddChild(constrSignMesh);
        SetRelPos(icon, construction.Pos, data);
        return icon;
    }

    protected override IEnumerable<Construction> GetKeys(Data data)
    {
        return Chunk.Polys
            .Where(p => data.Infrastructure.CurrentConstruction.ByPoly.ContainsKey(p.Id))
            .SelectMany(p => data.Infrastructure.CurrentConstruction.ByPoly[p.Id]);
    }

    protected override bool Ignore(Construction element, Data data)
    {
        return false;
    }
}
