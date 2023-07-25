using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RegimeBordersNode : BorderChunkNode
{
    public RegimeBordersNode(MapChunk chunk, Func<MapPolygon, int> getMarker, float thickness, Data data) 
        : base(nameof(RegimeBordersNode), chunk, getMarker, 
            p => p.Regime.Fulfilled() ? p.Regime.Entity(data).SecondaryColor : Colors.Transparent,
            thickness, data)
    {
    }
}
