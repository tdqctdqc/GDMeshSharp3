using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ConstructionIconLayer : MapChunkGraphicLayer<Construction>
{
    private static Texture2D _underConstruction => TextureManager.Textures["UnderConstruction"];
    public ConstructionIconLayer(MapChunk chunk, Data data, MapGraphics mg) 
        : base(nameof(ConstructionIconLayer), data, chunk, new Vector2(0f, .5f), mg.ChunkChangedCache.ConstructionsChanged)
    {        
    }
    private ConstructionIconLayer() : base()
    {
    }
    protected override Node2D MakeGraphic(Construction key, Data data)
    {
        var construction = key;
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
}
