using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RegimeBordersNode : BorderChunkNode
{
    public RegimeBordersNode(MapChunk chunk, float thickness, Data data) 
        : base(nameof(RegimeBordersNode), chunk, 
            (p, n) => p.Regime.RefId == n.Regime.RefId,
            p => p.Regime.Fulfilled() ? p.Regime.Entity(data).SecondaryColor : Colors.Transparent,
            (m, n) => thickness, data)
    {
    }

    
}
