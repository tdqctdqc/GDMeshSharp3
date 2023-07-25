using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RegimeFillNode : PolyFillChunkGraphic
{
    public RegimeFillNode(MapChunk chunk, Data data) 
        : base(nameof(RegimeFillNode), chunk, 
            (p, d) => p.Regime.Fulfilled() ? p.Regime.Entity(d).PrimaryColor : Colors.Transparent, 
            data)
    {
    }
}
