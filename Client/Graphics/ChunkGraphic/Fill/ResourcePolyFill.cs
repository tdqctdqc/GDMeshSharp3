using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ResourcePolyFill : PolyFillChunkGraphic
{
    public ResourcePolyFill(MapChunk chunk, Data data) 
        : base(nameof(ResourcePolyFill), chunk,
            (p, d) =>
               {
                   var rs = p.GetResourceDeposits(d);
                   if (rs == null || rs.Count == 0)
                   {
                       return p.IsLand
                           ? Colors.White
                           : data.Models.Landforms.Sea.Color;
                   }
                   if (rs.Count > 1) return Colors.Green;
                   return rs.First().Item.Model(d).Color;
               }, data)
    {
    }
}
