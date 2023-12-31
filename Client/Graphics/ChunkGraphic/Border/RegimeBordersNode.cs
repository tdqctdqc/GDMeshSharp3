using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RegimeBordersNode : BorderChunkNode
{
    private static float _thickness = 20f;
    public RegimeBordersNode(MapChunk chunk, Data data) 
        : base(nameof(RegimeBordersNode), chunk, data)
    {
    }
    protected override bool InUnion(MapPolygon p1, MapPolygon p2, Data data)
    {
        return p1.OwnerRegime.RefId == p2.OwnerRegime.RefId;
    }

    protected override float GetThickness(MapPolygon p1, MapPolygon p2, Data data)
    {
        return _thickness;
    }

    protected override Color GetColor(MapPolygon p, Data data)
    {
        return p.OwnerRegime.Fulfilled() ? p.OwnerRegime.Entity(data).PrimaryColor : Colors.Transparent;
    }
}
