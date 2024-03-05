
using System.Collections.Generic;
using Godot;

public partial class VegetationFillChunkGraphic 
    : PolyCellFillChunkGraphic
{
    public VegetationFillChunkGraphic(MapChunk chunk,
        Data data) 
        : base("Vegetation", chunk, 
            LayerOrder.Terrain,
            new Vector2(0f, 1f),
            data)
    {
    }

    public override Color GetColor(Cell cell, Data d)
    {
        return cell.GetVegetation(d)
            .Color.Darkened(cell.GetLandform(d).DarkenFactor)
            .Darkened(Game.I.Random.RandfRange(-TerrainChunkModule.ColorWobble, TerrainChunkModule.ColorWobble))
            ;
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