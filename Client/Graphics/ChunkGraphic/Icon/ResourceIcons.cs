using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ResourceIcons 
    : ChunkIconsMultiMesh<Item, ResourceDeposit>
{
    public ResourceIcons(MapChunk chunk, Data d) 
        : base("Resources", chunk, MeshExt.GetQuadMesh(Vector2.One * 25f))
    {
        Draw(d);
    }

    protected override Texture2D GetTexture(Item t)
    {
        return t.Icon.Texture;
    }

    protected override IEnumerable<ResourceDeposit> GetElements(Data d)
    {
        return Chunk.Polys
            .Select(p => p.GetResourceDeposits(d))
            .Where(r => r != null)
            .SelectMany(r => r);
    }

    protected override Item GetModel(ResourceDeposit t, Data d)
    {
        return t.Item.Model(d);
    }

    protected override Vector2 GetWorldPos(ResourceDeposit t, Data d)
    {
        return t.Poly.Entity(d).Center;
    }
}
