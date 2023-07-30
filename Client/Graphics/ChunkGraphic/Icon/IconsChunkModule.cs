using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class IconsChunkModule : MapChunkGraphicModule
{
    public BuildingIcons BuildingIcons { get; private set; }
    public SettlementIcons SettlementIcons { get; private set; }
    public ConstructionIcons ConstructionIcons { get; private set; }
    public IconsChunkModule(MapChunk chunk, Data data) : base(chunk, nameof(IconsChunkModule))
    {
        ConstructionIcons = new ConstructionIcons(chunk, data);
        AddNode(ConstructionIcons);

        SettlementIcons = new SettlementIcons(chunk, data);
        AddNode(SettlementIcons);
        
        BuildingIcons = new BuildingIcons(chunk, data);
        AddNode(BuildingIcons);
    }

    public static ChunkGraphicLayer<IconsChunkModule> GetLayer(Data d, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<IconsChunkModule>("Icons", segmenter,
            c => new IconsChunkModule(c, d),
            d);
        l.RegisterForEntityLifetime(n => n.Position.Poly(d).GetChunk(d), 
            m => m.BuildingIcons, d);
        
        l.RegisterForEntityLifetime(n => n.Poly.Entity(d).GetChunk(d), 
            m => m.SettlementIcons, d);
        l.RegisterForChunkNotice(d.Infrastructure.SettlementAux.ChangedTier, 
            n => ((Settlement)n.Entity).Poly.Entity(d).GetChunk(d),
            (notice, graphic) => graphic.SettlementIcons.QueueChange(notice.Entity));
        
        l.RegisterForAdd(d.Infrastructure.ConstructionAux.StartedConstruction,
            k => k.Pos.Poly(d).GetChunk(d),
            n => n.ConstructionIcons);
        l.RegisterForRemove(d.Infrastructure.ConstructionAux.EndedConstruction,
            k => k.Pos.Poly(d).GetChunk(d),
            n => n.ConstructionIcons);

        return l;
    }
}
