using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class SettlementIcons 
    : ChunkIconsMultiMesh<SettlementTier, Cell>
{
    
    public SettlementIcons(MapChunk chunk, 
        Vector2 zoomVisibilityRange,
        Data d) 
        : base("Settlements", zoomVisibilityRange,
            chunk, MeshExt.GetQuadMesh(Vector2.One * 50f))
    {
    }
    protected override IEnumerable<Cell> GetElements(Data data)
    {
        return Chunk.Cells.Where(p => p.HasSettlement(data))
            .Where(c => c.Landform.Get(data) == data.Models.Landforms.Urban);
    }
    protected override Texture2D GetTexture(SettlementTier t)
    {
        return t.Icon.Texture;
    }

    protected override SettlementTier GetModel(Cell t, Data d)
    {
        return t.GetSettlement(d).Tier.Get(d);
    }

    protected override Vector2 GetWorldPos(Cell t, Data d)
    {
        return t.GetCenter();
    }

    public override void RegisterForRedraws(Data d)
    {
        this.RegisterDrawOnTick(d);
    }
    public override Settings GetSettings(Data d)
    {
        var settings = new Settings(Name);
        settings.SettingsOptions.Add(
            this.MakeVisibilitySetting(true));
        
        return settings;
    }
}

