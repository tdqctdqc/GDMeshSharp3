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
        SettlementIcons = new SettlementIcons(chunk, data);
        BuildingIcons = new BuildingIcons(chunk, data);
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
