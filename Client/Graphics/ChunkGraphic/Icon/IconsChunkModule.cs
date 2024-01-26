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
    
    public static ChunkGraphicLayer<IconsChunkModule> GetLayer(Client client, GraphicsSegmenter segmenter)
    {
        var d = client.Data;
        var l = new ChunkGraphicLayer<IconsChunkModule>(
            LayerOrder.Icons, "Icons", segmenter,
            c => new IconsChunkModule(c, d),
            d);
        
        l.RegisterForAdd(d.Infrastructure.ConstructionAux.StartedConstruction,
            k => PlanetDomainExt.GetPolyCell(k.PolyCellId, d).GetChunk(d),
            n => n.ConstructionIcons);
        l.RegisterForRemove(d.Infrastructure.ConstructionAux.EndedConstruction,
            k => PlanetDomainExt.GetPolyCell(k.PolyCellId, d).GetChunk(d),
            n => n.ConstructionIcons);
        
        d.Notices.Ticked.Blank.Subscribe(() =>
        {
            client.QueuedUpdates.Enqueue(() =>
            {
                foreach (var kvp in l.ByChunkCoords)
                {
                    var v = kvp.Value;
                    v.SettlementIcons.Draw(d);
                    v.BuildingIcons.Draw(d);
                }
            });
        });
        
        return l;
    }
}
