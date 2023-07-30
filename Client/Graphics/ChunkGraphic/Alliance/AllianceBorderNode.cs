using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class AllianceBorderNode : BorderChunkNode
{
    public AllianceBorderNode(MapChunk chunk, float thickness, Data data) 
        : base(nameof(AllianceBorderNode), chunk,
            (n, p) =>
            {
                if (n.Regime.Fulfilled() != p.Regime.Fulfilled()) return false;
                if (n.Regime.Fulfilled() == p.Regime.Fulfilled() == false) return true;
                return p.Regime.Entity(data).GetAlliance(data) 
                       == n.Regime.Entity(data).GetAlliance(data);
            },
        p => p.Regime.Fulfilled() 
                ? p.Regime.Entity(data).GetAlliance(data).Leader.Entity(data).PrimaryColor 
                : Colors.Transparent, 
            thickness, 
            data)
    {
    }
}
