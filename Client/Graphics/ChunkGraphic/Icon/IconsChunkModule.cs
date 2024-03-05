using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class IconsChunkModule : ChunkGraphicMultiModule
{
    public BuildingIcons BuildingIcons { get; private set; }
    public SettlementIcons SettlementIcons { get; private set; }
    public IconsChunkModule(MapChunk chunk, Data data)
    {
        ZIndex = (int)LayerOrder.Icons;
        SettlementIcons = new SettlementIcons(chunk,
            ChunkGraphic.IconZoomVisRange, data);
        BuildingIcons = new BuildingIcons(chunk, 
            ChunkGraphic.IconZoomVisRange, data);
        foreach (var m in GetModules())
        {
            AddChild(m.Node);
        }
    }

    protected override IEnumerable<IChunkGraphicModule> GetModules()
    {
        yield return SettlementIcons;
        yield return BuildingIcons;
    }
}
