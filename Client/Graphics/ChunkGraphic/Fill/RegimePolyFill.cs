using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RegimePolyFill : PolyFillChunkGraphic
{
    public RegimePolyFill(MapChunk chunk, 
        GraphicsSegmenter segmenter,
        Data data) 
        : base("Owner", chunk, 
            LayerOrder.PolyFill, segmenter, data)
    {
    }
    
    public override Color GetColor(MapPolygon poly, Data d)
    {
        return poly.OwnerRegime.Fulfilled()
            ? poly.OwnerRegime.Entity(d).GetMapColor()
            : Colors.Transparent;
    }
}
