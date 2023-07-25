using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class TerrainChunkModule : MapChunkGraphicModule
{
    public TerrainChunkModule(MapChunk chunk, Data data) : base(nameof(TerrainChunkModule))
    {
        var lfLayer = new PolyTriFillChunkGraphic("Landform", chunk, 
            (pt, d) => pt.Landform.Color);
        AddNode(lfLayer);
        var vegLayer = new PolyTriFillChunkGraphic("Vegetation", chunk,
            (pt, d) => pt.Vegetation.Color.Darkened(pt.Landform.DarkenFactor));
        AddNode(vegLayer);
    }
}
