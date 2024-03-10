
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ChunkLabelGraphics : Node2D, IChunkGraphicModule
{
    public string Name => "Labels";
    public Node2D Node => this;
    public ChunkGraphicModuleVisibility Visibility { get; }
    public MapChunk Chunk { get; set; }
    private Data _data;
    private bool _drawn;
    public ChunkLabelGraphics(MapChunk chunk, Vector2 zoomVisRange, Data d)
    {
        _data = d;
        Chunk = chunk;
        ZIndex = (int)LayerOrder.Labels;
        Visibility = new ChunkGraphicModuleVisibility(zoomVisRange);
        Visibility.SetVisibility += v =>
        {
            if (v == true) _drawn = false;
        };
    }

    public override void _Draw()
    {
        if (_drawn == false)
        {
            foreach (var (text, pos) in GetLabels(_data))
            {
                DrawString(UiThemes.DefaultTheme.DefaultFont, pos,
                    text);
            }

            _drawn = true;
        }
    }

    public void Draw(Data d)
    {
        _drawn = false;
    }

    private List<(string, Vector2)> GetLabels(Data d)
    {
        return Chunk.Polys.Where(p => p.HasSettlement(d))
            .Select(p =>
            {
                var urban = p.GetCells(d).Where(c => c.Landform.ModelId == d.Models.Landforms.Urban.Id)
                    .FirstOrDefault();
                Vector2 pos;
                if(urban is not null)
                {
                    pos = Chunk.RelTo.Center.Offset(urban.GetCenter(), d);
                }
                else
                {
                    pos = Chunk.RelTo.Center.Offset(p.Center, d);
                }
                return (p.GetSettlement(d).Name, pos);
            }).ToList();
    }
    public void RegisterForRedraws(Data d)
    {
        this.RegisterDrawOnTick(d);
    }

    public Settings GetSettings(Data d)
    {
        var settings = new Settings(Name);
        var visSetting = this.MakeVisibilitySetting(true);
        this.AddSettingEnforcement(visSetting, (v, g) =>
        {
            g._drawn = false;
        });
        settings.SettingsOptions.Add(visSetting);
        return settings;
    }
}