using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class SettlementIcons 
    : ChunkIconsMultiMesh<SettlementTier, PolyCell>
{
    public SettlementIcons(MapChunk chunk, Data d) 
        : base("Settlements", chunk, Vector2.One * 50f)
    {
        Draw(d);
    }
    protected override IEnumerable<PolyCell> GetElements(Data data)
    {
        return Chunk.Polys.Where(p => p.HasSettlement(data))
            .SelectMany(p => p.GetCells(data))
            .Where(c => c.Landform.Model(data) == data.Models.Landforms.Urban);
    }
    protected override Texture2D GetTexture(SettlementTier t)
    {
        return t.Icon.Texture;
    }

    protected override SettlementTier GetModel(PolyCell t, Data d)
    {
        var poly = ((LandCell)t).Polygon.Entity(d);
        return poly.GetSettlement(d).Tier.Model(d);
    }

    protected override Vector2 GetWorldPos(PolyCell t, Data d)
    {
        return t.GetCenter();
    }
}

