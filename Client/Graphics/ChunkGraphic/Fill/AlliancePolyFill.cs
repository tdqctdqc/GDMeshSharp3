using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class AlliancePolyFill : PolyFillChunkGraphic
{
    public AlliancePolyFill(MapChunk chunk, GraphicsSegmenter segmenter, Data data) 
        : base(nameof(AlliancePolyFill), chunk, LayerOrder.PolyFill,
            segmenter, data)
    {
    }

    public override Color GetColor(MapPolygon poly, Data d)
    {
        if(poly.OwnerRegime.Fulfilled() == false) return Colors.Transparent;
        return poly.OwnerRegime.Entity(d).GetAlliance(d).Leader.Entity(d).GetMapColor();
    }
}
