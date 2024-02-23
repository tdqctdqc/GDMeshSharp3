using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class MapPolygonEdgeExt
{
    public static Vector2 Position(this MapPolygonEdge edge, Data d)
    {
        var p1 = edge.HighPoly.Entity(d);
        var p2 = edge.LowPoly.Entity(d);
        return (p1.Center + p1.GetOffsetTo(p2, d)).ClampPosition(d);
    }

    
    public static MapPolygon GetOtherPoly(this MapPolygonEdge b, MapPolygon p, Data data)
    {
        if (p == b.LowPoly.Entity(data)) return b.HighPoly.Entity(data);
        if (p == b.HighPoly.Entity(data)) return b.LowPoly.Entity(data);
        throw new Exception();
    }

    public static float GetAvgMoisture(this MapPolygonEdge e, Data data)
    {
        return (e.HighPoly.Entity(data).Moisture + e.LowPoly.Entity(data).Moisture) / 2f;
    }
    public static float GetAvgRoughness(this MapPolygonEdge e, Data data)
    {
        return (e.HighPoly.Entity(data).Roughness + e.LowPoly.Entity(data).Roughness) / 2f;
    }


    public static IEnumerable<MapPolygonEdge> GetIncidentEdges(this MapPolygonEdge e, Data data)
    {
        var n1 = e.HiNexus.Entity(data).IncidentEdges.Items(data).Where(n => n != e);
        var n2 = e.LoNexus.Entity(data).IncidentEdges.Items(data).Where(n => n != e);
        if (n1 == null || n2 == null) return new List<MapPolygonEdge>();
        return n1.Union(n2).Distinct();
    }
    public static bool IsLandToSeaEdge(this MapPolygonEdge edge, Data data)
    {
        var hi = edge.HiNexus.Entity(data);
        var incHi = hi.IncidentPolys.Items(data);
        var lo = edge.LoNexus.Entity(data);
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
        return edge.HighPoly.Entity(d).IsLand != edge.LowPoly.Entity(d).IsLand;
    }
    public static bool EdgeToPoly(this MapPolygonEdge edge, MapPolygon poly)
    {
        return edge.HighPoly.RefId == poly.Id || edge.LowPoly.RefId == poly.Id;
    }

}