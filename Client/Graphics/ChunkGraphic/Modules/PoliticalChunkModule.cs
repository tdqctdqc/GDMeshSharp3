
using System.Collections.Generic;
using Godot;

public partial class PoliticalChunkModule 
    : ChunkGraphicMultiModule
{
    private PoliticalFillModule _fill;
    private PoliticalBordersModule _border;
    public enum Mode
    {
        Regime, Alliance, Diplomacy
    }
    public Mode SelectedMode { get; private set; }
    public PoliticalChunkModule(MapChunk chunk, 
        Data d) : base("Political", new Vector2(0f, 1f))
    {
        SelectedMode = Mode.Regime;
        _fill = new PoliticalFillModule(this, chunk, d);
        _border = new PoliticalBordersModule(this, chunk, d);
        foreach (var m in GetModules())
        {
            AddChild(m.Node);
        }
    }

    protected override IEnumerable<IChunkGraphicModule> GetModules()
    {
        yield return _fill;
        yield return _border;
    }

    public override Settings GetSettings(Data d)
    {
        var settings = new Settings(Name);
        settings.SettingsOptions.Add(
            this.MakeVisibilitySetting(true));
        settings.SettingsOptions.Add(
            this.MakeTransparencySetting());
        
        return settings;
    }
}