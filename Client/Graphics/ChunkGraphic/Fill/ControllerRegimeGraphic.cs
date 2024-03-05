
using System.Collections.Generic;
using Godot;

public partial class ControllerRegimeGraphic : ChunkGraphicMultiModule
{
    private ControllerRegimePolyCellFill _fill;
    private ControllerPolyCellBorder _borders;
    public ControllerRegimeGraphic(MapChunk chunk, 
        GraphicsSegmenter segmenter, Data data) 
        : base("Controller", new Vector2(0f, 1f))
    {
        _fill = new ControllerRegimePolyCellFill(chunk, data);
        _borders = new ControllerPolyCellBorder(chunk, data);
        foreach (var m in GetModules())
        {
            AddChild(m.Node);
        }
    }

    protected override IEnumerable<IChunkGraphicModule> GetModules()
    {
        yield return _fill;
        yield return _borders;
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