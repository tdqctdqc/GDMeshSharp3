using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class TerrainChunkModule : MapChunkGraphicModule
{
    public TerrainChunkModule(MapChunk chunk, Data data) : base(chunk, nameof(TerrainChunkModule))
    {
        var colorWobble = .05f;
        var lfLayer = new PolyCellFillChunkGraphic("Landform", chunk, 
            (pt, d) => pt.GetLandform(data)
                .Color.Darkened(Game.I.Random.RandfRange(-colorWobble, colorWobble)), data);
        AddNode(lfLayer);
        var vegLayer = new PolyCellFillChunkGraphic("Vegetation", chunk,
            (pt, d) => pt.GetVegetation(data)
                .Color.Darkened(pt.GetLandform(data).DarkenFactor).Darkened(Game.I.Random.RandfRange(-colorWobble, colorWobble)),
                    data);
        AddNode(vegLayer);
    }
}
