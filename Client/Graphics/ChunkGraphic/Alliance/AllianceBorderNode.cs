using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class AllianceBorderNode : BorderChunkNode
{
    public AllianceBorderNode(MapChunk chunk, float thickness, Data data) 
        : base(nameof(AllianceBorderNode), chunk, 
            p => p.Regime.RefId,
            p => p.Regime.Fulfilled() 
                ? p.Regime.Entity(data).GetAlliance(data).Leader.Entity(data).PrimaryColor 
                : Colors.Transparent, thickness, data)
    {
    }
}
