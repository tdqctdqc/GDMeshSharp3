using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RegimePolyFill : PolyFillChunkGraphic
{
    public RegimePolyFill(MapChunk chunk, Data data) 
        : base(nameof(RegimePolyFill), chunk, 
            (p, d) => p.Regime.Fulfilled() ? p.Regime.Entity(d).PrimaryColor : Colors.Transparent, 
            data)
    {
    }
}
