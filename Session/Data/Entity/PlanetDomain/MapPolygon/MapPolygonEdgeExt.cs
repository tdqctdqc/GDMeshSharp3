using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class MapPolygonEdgeExt
{
    public static List<LineSegment> GetSegsAbs(this MapPolygonEdge b)
    {
        return b.HighSegsRel().Segments
            .Select(s => s.Translate(b.HighPoly.Entity().Center))
            .ToList().Ordered<LineSegment, Vector2>();
    }

    public static (MapPolyNexus from, MapPolyNexus to) OrderNexi(this MapPolygonEdge edge, MapPolygon poly, Data data)
    {
        var edgeSegs = edge.GetSegsRel(poly).Segments;
        var fromP = edgeSegs.First().From;
        var toP = edgeSegs.Last().To;
        
        var hiNexusP = poly.GetOffsetTo(edge.HiNexus.Entity().Point, data);
        var loNexusP = poly.GetOffsetTo(edge.LoNexus.Entity().Point, data);

        MapPolyNexus from;
        MapPolyNexus to;
        if (hiNexusP == fromP
            && loNexusP == toP)
        {
            from = edge.HiNexus.Entity();
            to = edge.LoNexus.Entity();
        }
        else if (hiNexusP == toP
                 && loNexusP == fromP)
        {
            to = edge.HiNexus.Entity();
            from = edge.LoNexus.Entity();
        } else { throw new Exception("bad edge nexi"); }

        return (from, to);
    }
    public static PolyBorderChain GetSegsRel(this MapPolygonEdge b, MapPolygon p)
    {
        if (b.HighPoly.Entity() == p)
        {
            return b.HighSegsRel();
        }
        if (b.LowPoly.Entity() == p)
        {
            return b.LowSegsRel();
        }

        throw new Exception();
    }

    public static MapPolygon GetOtherPoly(this MapPolygonEdge b, MapPolygon p)
    {
        if (p == b.LowPoly.Entity()) return b.HighPoly.Entity();
        if (p == b.HighPoly.Entity()) return b.LowPoly.Entity();
        throw new Exception();
    }
    public static bool IsRegimeBorder(this MapPolygonEdge b)
    {
        return b.HighPoly.Entity().Regime.RefId != b.LowPoly.Entity().Regime.RefId;
    }

    public static float GetAvgMoisture(this MapPolygonEdge e)
    {
        return (e.HighPoly.Entity().Moisture + e.LowPoly.Entity().Moisture) / 2f;
    }
    public static float GetAvgRoughness(this MapPolygonEdge e)
    {
        return (e.HighPoly.Entity().Roughness + e.LowPoly.Entity().Roughness) / 2f;
    }

    public static float GetLength(this MapPolygonEdge e)
    {
        return e.HighSegsRel().Segments.Sum(s => s.Length());
    }

    public static IEnumerable<MapPolygonEdge> GetIncidentEdges(this MapPolygonEdge e)
    {
        var n1 = e.HiNexus.Entity().IncidentEdges.Entities().Where(n => n != e);
        var n2 = e.LoNexus.Entity().IncidentEdges.Entities().Where(n => n != e);
        if (n1 == null || n2 == null) return new List<MapPolygonEdge>();
        return n1.Union(n2).Distinct();
    }
    public static bool IsLandToSeaEdge(this MapPolygonEdge edge)
    {
        var w1 = edge.HiNexus.Entity().IncidentPolys.Any(p => p.IsWater());
        var w2 = edge.LoNexus.Entity().IncidentPolys.Any(p => p.IsWater());
        return (w1 || w2) && (!w1 || !w2);
    }

    public static bool IsRiver(this MapPolygonEdge edge)
    {
        var width = River.GetWidthFromFlow(edge.MoistureFlow);
        return width >= River.WidthFloor;
    }

    public static bool EdgeToPoly(this MapPolygonEdge edge, MapPolygon poly)
    {
        return edge.HighPoly.RefId == poly.Id || edge.LowPoly.RefId == poly.Id;
    }
}