using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ResourceIcons 
    : ChunkIconsMultiMesh<Item, ResourceDeposit>
{
    private Vector2 _zoomVisibilityRange;

    public ResourceIcons(MapChunk chunk, Vector2 zoomVisibilityRange,
        Data d) 
        : base("Resources", chunk, MeshExt.GetQuadMesh(Vector2.One * 25f))
    {
        ZIndex = (int)LayerOrder.Icons;
        _zoomVisibilityRange = zoomVisibilityRange;
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

    public override void RegisterForRedraws(Data d)
    {
        this.RegisterDrawOnTick(d);
    }
    public override void DoUiTick(UiTickContext context, Data d)
    {
        var zoom = context.ZoomLevel;
        if (_zoomVisibilityRange.X > zoom || _zoomVisibilityRange.Y < zoom)
        {
            Visible = false;
        }
        else
        {
            Visible = true;
        }
    }
}
