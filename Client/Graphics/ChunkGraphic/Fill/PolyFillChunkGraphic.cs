using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class PolyFillChunkGraphic : MapChunkGraphicModule
{
    public PolyFillChunkGraphic(MapChunk chunk, Data data, Func<MapPolygon, Color> getPolyColor,
        float transparency = 1f)
    {
        var polyLayer = new PolyFillLayer(chunk, data, getPolyColor, transparency);
        AddLayer(new Vector2(0f, 1f), polyLayer);
    }

    private PolyFillChunkGraphic()
    {
        
    }
}