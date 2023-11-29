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
        return d.Planet.ClampPosition(p1.Center + p1.GetOffsetTo(p2, d));
    }
    public static List<LineSegment> GetSegsAbs(this MapPolygonEdge b, Data data)
    {
        return b.HighSegsRel(data).Segments
            .Select(s => s.Translate(b.HighPoly.Entity(data).Center))
            .ToList().Ordered<LineSegment, Vector2>();
    }

    public static (MapPolyNexus from, MapPolyNexus to) OrderNexi(this MapPolygonEdge edge, MapPolygon poly, Data data)
    {
        var edgeSegs = edge.GetSegsRel(poly, data).Segments;
        var fromP = edgeSegs.First().From;
        var toP = edgeSegs.Last().To;
        
        var hiNexusP = poly.GetOffsetTo(edge.HiNexus.Entity(data).Point, data);
        var loNexusP = poly.GetOffsetTo(edge.LoNexus.Entity(data).Point, data);

        MapPolyNexus from;
        MapPolyNexus to;
        if (hiNexusP == fromP
            && loNexusP == toP)
        {
            from = edge.HiNexus.Entity(data);
            to = edge.LoNexus.Entity(data);
        }
        else if (hiNexusP == toP
                 && loNexusP == fromP)
        {
            to = edge.HiNexus.Entity(data);
            from = edge.LoNexus.Entity(data);
        } else { throw new Exception("bad edge nexi"); }

        return (from, to);
    }
    public static PolyBorderChain GetSegsRel(this MapPolygonEdge b, MapPolygon p, Data data)
    {
        if (b.HighPoly.Entity(data) == p)
        {
            return b.HighSegsRel(data);
        }
        if (b.LowPoly.Entity(data) == p)
        {
            return b.LowSegsRel(data);
        }

        throw new Exception();
    }

    public static MapPolygon GetOtherPoly(this MapPolygonEdge b, MapPolygon p, Data data)
    {
        if (p == b.LowPoly.Entity(data)) return b.HighPoly.Entity(data);
        if (p == b.HighPoly.Entity(data)) return b.LowPoly.Entity(data);
        throw new Exception();
    }
    public static bool IsRegimeBorder(this MapPolygonEdge b, Data data)
    {
        return b.HighPoly.Entity(data).OwnerRegime.RefId != b.LowPoly.Entity(data).OwnerRegime.RefId;
    }

    public static float GetAvgMoisture(this MapPolygonEdge e, Data data)
    {
        return (e.HighPoly.Entity(data).Moisture + e.LowPoly.Entity(data).Moisture) / 2f;
    }
    public static float GetAvgRoughness(this MapPolygonEdge e, Data data)
    {
        return (e.HighPoly.Entity(data).Roughness + e.LowPoly.Entity(data).Roughness) / 2f;
    }

    public static float GetLength(this MapPolygonEdge e, Data data)
    {
        return e.HighSegsRel(data).Segments.Sum(s => s.Length());
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
        var w1 = edge.HiNexus.Entity(data).IncidentPolys.Items(data).Any(p => p.IsWater());
        var w2 = edge.LoNexus.Entity(data).IncidentPolys.Items(data).Any(p => p.IsWater());
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

    public static bool LineCrosses(this MapPolygonEdge edge, Vector2 absA, Vector2 absB, Data data)
    {
        var hiSegs = edge.HighSegsRel(data);
        var hi = edge.HighPoly.Entity(data);
        var relA = hi.GetOffsetTo(absA, data);
        var relB = hi.GetOffsetTo(absB, data);
        return hiSegs.Segments.Any(s => s.IntersectsInclusive(relA, relB));
    }

    public static bool PointWithinDist(this MapPolygonEdge edge,
        Vector2 pAbs, float dist, Data d)
    {
        var relPoly = edge.HighPoly.Entity(d);
        var pRel = d.Planet.GetOffsetTo(relPoly.Center, pAbs);
        return edge.GetSegsRel(relPoly, d)
            .Segments.Any(s => s.DistanceTo(pRel) < dist);
    }
}