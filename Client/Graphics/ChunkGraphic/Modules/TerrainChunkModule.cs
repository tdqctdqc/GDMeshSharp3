using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class TerrainChunkModule : MapChunkGraphicModule
{
    public TerrainChunkModule(MapChunk chunk, Data data) : base(chunk, nameof(TerrainChunkModule))
    {
        var lfLayer = new PolyTriFillChunkGraphic("Landform", chunk, 
            (pt, d) => pt.Landform(data).Color, data);
        AddNode(lfLayer);
        var vegLayer = new PolyTriFillChunkGraphic("Vegetation", chunk,
            (pt, d) => pt.Vegetation(data).Color.Darkened(pt.Landform(data).DarkenFactor), data);
        AddNode(vegLayer);
    }
}
