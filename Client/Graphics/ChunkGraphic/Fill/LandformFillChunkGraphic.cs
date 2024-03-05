
using System.Collections.Generic;
using Godot;

public partial class LandformFillChunkGraphic : PolyCellFillChunkGraphic
{
    public LandformFillChunkGraphic(MapChunk chunk, 
        Data data) 
        : base("Landform", chunk, 
            LayerOrder.Terrain,
            new Vector2(0f, 1f),
            data)
    {
    }

    public override Color GetColor(Cell cell, Data d)
    {
        return cell.GetLandform(d)
            .Color
            .Darkened(Game.I.Random.RandfRange(-TerrainChunkModule.ColorWobble, TerrainChunkModule.ColorWobble));
    }

    public override void RegisterForRedraws(Data d)
    {
        
    }

    public override Settings GetSettings(Data d)
    {
        var settings = new Settings(Name);
        settings.SettingsOptions.Add(
            this.MakeVisibilitySetting(true));
        var multiSettings = new MultiSettings(Name,
            new List<ISettings>
            {
                new Settings(Name)
            }
        );
        return settings;
    }
}