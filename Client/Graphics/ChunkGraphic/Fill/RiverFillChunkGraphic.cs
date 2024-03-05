
using System.Collections.Generic;
using Godot;

public partial class RiverFillChunkGraphic 
    : PolyCellFillChunkGraphic
{
    private static Color _riverColor;
    public RiverFillChunkGraphic(MapChunk chunk,
        Data data) 
        : base("River", chunk, 
            LayerOrder.Rivers, 
            new Vector2(0f, 1f), data)
    {
        _riverColor = data.Models.Landforms.River
            .Color;
    }

    public override Color GetColor(Cell cell, Data d)
    {
        if(cell is RiverCell)
        {
            return _riverColor
                .Darkened(Game.I.Random
                    .RandfRange(-TerrainChunkModule.ColorWobble,
                        TerrainChunkModule.ColorWobble));
        }
        return Colors.Transparent;
    }

    public override void RegisterForRedraws(Data d)
    {
        
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