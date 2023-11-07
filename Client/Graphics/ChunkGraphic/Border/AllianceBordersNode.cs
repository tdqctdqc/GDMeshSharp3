using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class AllianceBordersNode : BorderChunkNode
{
    public AllianceBordersNode(MapChunk chunk, Data data, bool primaryColor) : base(
            nameof(AllianceBordersNode), chunk, data)
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
        if (m.OwnerRegime.RefId == -1 || n.OwnerRegime.RefId == -1) return 30f;
        if (m.OwnerRegime.Entity(data).GetAlliance(data) 
            == n.OwnerRegime.Entity(data).GetAlliance(data))
        {
            return 5f;
        }
        return 30f;
    }

    protected override bool InUnion(MapPolygon p1, MapPolygon p2, Data data)
    {
        return p1.OwnerRegime.RefId == p2.OwnerRegime.RefId;
    }
}
