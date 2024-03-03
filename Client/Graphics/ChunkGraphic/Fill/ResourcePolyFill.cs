using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ResourcePolyFill : PolyFillChunkGraphic
{
    public ResourcePolyFill(MapChunk chunk, 
        GraphicsSegmenter segmenter,
        Data data) 
        : base(nameof(ResourcePolyFill), chunk,
            LayerOrder.Resources, segmenter, data)
    {
    }
    
    public override Color GetColor(MapPolygon poly, Data d)
    {
        var rs = poly.GetResourceDeposits(d);
        if (rs == null || rs.Count == 0)
        {
            return poly.IsLand
                ? Colors.White
                : d.Models.Landforms.Sea.Color;
        }
        if (rs.Count > 1) return Colors.Green;
        return rs.First().Item.Model(d).Color;
    }
}
