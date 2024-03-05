using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class TerrainChunkModule : ChunkGraphicMultiModule
{
    public static float ColorWobble = .05f;

    private LandformFillChunkGraphic _lf;
    private VegetationFillChunkGraphic _v;
    private RiverFillChunkGraphic _r;
    
    public TerrainChunkModule(MapChunk chunk, Data data)
        : base("Terrain", new Vector2(0f, 1f))
    {
        _lf = new LandformFillChunkGraphic(chunk, data);
        _v = new VegetationFillChunkGraphic(chunk, data);
        _r = new RiverFillChunkGraphic(chunk, data);
        foreach (var m in GetModules())
        {
            AddChild(m.Node);
        }
    }

    protected override IEnumerable<IChunkGraphicModule> GetModules()
    {
        yield return _lf;
        yield return _v;
        yield return _r;
    }

    public override Settings GetSettings(Data d)
    {
        var settings = new Settings(Name);
        settings.SettingsOptions.Add(
            this.MakeVisibilitySetting(true));
        return settings;
    }
}
