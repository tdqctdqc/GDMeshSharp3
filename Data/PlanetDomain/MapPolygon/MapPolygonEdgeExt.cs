using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class MapPolygonEdgeExt
{
    public static Vector2 Position(this MapPolygonEdge edge, Data d)
    {
        var p1 = edge.HighPoly.Get(d);
        var p2 = edge.LowPoly.Get(d);
        return (p1.Center + p1.GetOffsetTo(p2, d)).ClampPosition(d);
    }

    
    public static MapPolygon GetOtherPoly(this MapPolygonEdge b, MapPolygon p, Data data)
    {
        if (p == b.LowPoly.Get(data)) return b.HighPoly.Get(data);
        if (p == b.HighPoly.Get(data)) return b.LowPoly.Get(data);
        throw new Exception();
    }

    public static float GetAvgMoisture(this MapPolygonEdge e, Data data)
    {
        return (e.HighPoly.Get(data).Moisture + e.LowPoly.Get(data).Moisture) / 2f;
    }
    public static float GetAvgRoughness(this MapPolygonEdge e, Data data)
    {
        return (e.HighPoly.Get(data).Roughness + e.LowPoly.Get(data).Roughness) / 2f;
    }


    public static IEnumerable<MapPolygonEdge> GetIncidentEdges(this MapPolygonEdge e, Data data)
    {
        var n1 = e.HiNexus.Get(data).IncidentEdges.Items(data).Where(n => n != e);
        var n2 = e.LoNexus.Get(data).IncidentEdges.Items(data).Where(n => n != e);
        if (n1 == null || n2 == null) return new List<MapPolygonEdge>();
        return n1.Union(n2).Distinct();
    }
    public static bool IsLandToSeaEdge(this MapPolygonEdge edge, Data data)
    {
        var hi = edge.HiNexus.Get(data);
        var incHi = hi.IncidentPolys.Items(data);
        var lo = edge.LoNexus.Get(data);
        var incLo = lo.IncidentPolys.Items(data);
        var w1 = incHi.Any(p => p.IsWater());
        var w2 = incLo.Any(p => p.IsWater());
        return (w1 || w2) && (!w1 || !w2);
    }

    public static bool IsRiver(this MapPolygonEdge edge)
    {
        var width = River.GetWidthFromFlow(edge.MoistureFlow);
        return width >= River.WidthFloor;
    }
    public static bool IsCoast(this MapPolygonEdge edge, Data d)
    {
        return edge.HighPoly.Get(d).IsLand != edge.LowPoly.Get(d).IsLand;
    }
    public static bool EdgeToPoly(this MapPolygonEdge edge, MapPolygon poly)
    {
        return edge.HighPoly.RefId == poly.Id || edge.LowPoly.RefId == poly.Id;
    }

}