using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class IconsChunkModule : ChunkGraphicMultiModule
{
    public SettlementIcons SettlementIcons { get; private set; }
    public ResourceExtractionIcons ResourceExtractionIcons { get; private set; }
    public IconsChunkModule(MapChunk chunk, Data data)
        : base("Icons", ChunkGraphic.IconZoomVisRange)
    {
        ZIndex = (int)LayerOrder.Icons;
        SettlementIcons = new SettlementIcons(chunk,
            ChunkGraphic.IconZoomVisRange, data);
        ResourceExtractionIcons = new ResourceExtractionIcons(chunk,
            ChunkGraphic.IconZoomVisRange, data);
        foreach (var m in GetModules())
        {
            AddChild(m.Node);
        }
    }

    protected override IEnumerable<IChunkGraphicModule> GetModules()
    {
        yield return SettlementIcons;
        yield return ResourceExtractionIcons;
    }
    public override Settings GetSettings(Data d)
    {
        var settings = new Settings(Name);
        settings.SettingsOptions.Add(
            this.MakeVisibilitySetting(true));
        return settings;
    }
}
