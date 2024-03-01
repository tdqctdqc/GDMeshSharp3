using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class TerrainChunkModule : MapChunkGraphicModule
{
    public static float ColorWobble = .05f;
    public static TerrainChunkModule GetBase(MapChunk chunk, Data data)
    {
        var m = new TerrainChunkModule(chunk, data);
        var lfLayer = new PolyCellFillChunkGraphic("Landform", chunk, 
            (pt, d) => pt.GetLandform(data)
                .Color.Darkened(Game.I.Random.RandfRange(-ColorWobble, ColorWobble)), data);
        m.AddNode(lfLayer);
        var vegLayer = new PolyCellFillChunkGraphic("Vegetation", chunk,
            (pt, d) => pt.GetVegetation(data)
                .Color.Darkened(pt.GetLandform(data).DarkenFactor)
                .Darkened(Game.I.Random.RandfRange(-ColorWobble, ColorWobble)),
            data);
        m.AddNode(vegLayer);
        return m;
    }
    public static TerrainChunkModule GetRiver(MapChunk chunk, Data data)
    {
        var m = new TerrainChunkModule(chunk, data);
        var riverLayer = new PolyCellFillChunkGraphic("River", chunk,
            c => c is RiverCell,
            (pt, d) => data.Models.Landforms.River.Color
                .Darkened(Game.I.Random.RandfRange(-ColorWobble, ColorWobble)),
            data);
        m.AddNode(riverLayer);
        return m;
    }
    private TerrainChunkModule(MapChunk chunk, Data data) : base(chunk, nameof(TerrainChunkModule))
    {
    }

    
}
