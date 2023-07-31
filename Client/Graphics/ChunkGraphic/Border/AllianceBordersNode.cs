using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class AllianceBordersNode : BorderChunkNode
{
    public AllianceBordersNode(MapChunk chunk, Data data, bool primaryColor) : base(
            nameof(AllianceBordersNode), 
            chunk,
            (p, n) => p.Regime.RefId == n.Regime.RefId,
            p => GetColor(p, data, primaryColor),
            (m, n) => GetThickness(m,n,data), 
            data
        )
    {
    }

    private static Color GetColor(MapPolygon p, Data data, bool primary)
    {
        if(p.Regime.Fulfilled() == false) return Colors.Transparent;
        var allianceLeader = p.Regime.Entity(data).GetAlliance(data).Leader.Entity(data);
        return primary ? allianceLeader.PrimaryColor : allianceLeader.SecondaryColor;
    }

    private static float GetThickness(MapPolygon m, MapPolygon n, Data data)
    {
        if (m.Regime.RefId == -1 || n.Regime.RefId == -1) return 30f;
        if (m.Regime.Entity(data).GetAlliance(data) 
            == n.Regime.Entity(data).GetAlliance(data))
        {
            return 5f;
        }
        return 30f;
    }
}
