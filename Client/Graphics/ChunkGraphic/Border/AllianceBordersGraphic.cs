using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class AllianceBordersGraphic : PolyBorder
{
    public AllianceBordersGraphic(MapChunk chunk, Data data, bool primaryColor) : base(
            nameof(AllianceBordersGraphic), chunk, data)
    {
    }

    protected override Color GetColor(MapPolygon p, Data data)
    {
        if(p.OwnerRegime.Fulfilled() == false) return Colors.Transparent;
        var allianceLeader = p.OwnerRegime.Entity(data).GetAlliance(data).Leader.Entity(data);
        return allianceLeader.PrimaryColor;
    }

    protected override float GetThickness(MapPolygon m, MapPolygon n, Data data)
    {
        if (m.OwnerRegime.RefId == -1 || n.OwnerRegime.RefId == -1) 
            return 5f;
        if (m.OwnerRegime.Entity(data).GetAlliance(data) 
            == n.OwnerRegime.Entity(data).GetAlliance(data))
        {
            return 2.5f;
        }
        return 5f;
    }

    protected override bool InUnion(MapPolygon p1, MapPolygon p2, Data data)
    {
        return p1.OwnerRegime.RefId == p2.OwnerRegime.RefId;
    }
}
