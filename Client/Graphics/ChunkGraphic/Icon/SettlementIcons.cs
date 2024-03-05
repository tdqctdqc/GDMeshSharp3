using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class SettlementIcons 
    : ChunkIconsMultiMesh<SettlementTier, Cell>
{
    private Vector2 _zoomVisibilityRange;
    public SettlementIcons(MapChunk chunk, 
        Vector2 zoomVisibilityRange,
        Data d) 
        : base("Settlements", chunk, MeshExt.GetQuadMesh(Vector2.One * 50f))
    {
        _zoomVisibilityRange = zoomVisibilityRange;
    }
    protected override IEnumerable<Cell> GetElements(Data data)
    {
        return Chunk.Polys.Where(p => p.HasSettlement(data))
            .SelectMany(p => p.GetCells(data))
            .Where(c => c.Landform.Model(data) == data.Models.Landforms.Urban);
    }
    protected override Texture2D GetTexture(SettlementTier t)
    {
        return t.Icon.Texture;
    }

    protected override SettlementTier GetModel(Cell t, Data d)
    {
        var poly = ((LandCell)t).Polygon.Entity(d);
        return poly.GetSettlement(d).Tier.Model(d);
    }

    protected override Vector2 GetWorldPos(Cell t, Data d)
    {
        return t.GetCenter();
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

