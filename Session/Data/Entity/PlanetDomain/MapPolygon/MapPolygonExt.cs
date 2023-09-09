using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class MapPolygonExt
{
    public static PolyAuxData GetAuxData(this MapPolygon poly, Data data)
    {
        return data.Planet.PolygonAux.AuxDatas[poly];
    }
    public static Waypoint GetCenterWaypoint(this MapPolygon poly, Data data)
    {
        return data.Planet.Nav.GetPolyCenterWaypoint(poly);
    }
    public static IEnumerable<Waypoint> GetAssocWaypoints(this MapPolygon poly, Data data)
    {
        return data.Planet.Nav.GetPolyAssocWaypoints(poly, data);
    }
    public static bool PointInPolyAbs(this MapPolygon poly, Vector2 posAbs, Data data)
    {
        var posRel = poly.GetOffsetTo(posAbs, data);
        return data.Planet.PolygonAux.AuxDatas[poly].PointInPoly(poly, posRel, data);
    }
    public static bool PointInPolyRel(this MapPolygon poly, Vector2 posRel, Data data)
    {
        return data.Planet.PolygonAux.AuxDatas[poly].PointInPoly(poly, posRel, data);
    }
    public static Vector2 GetOffsetTo(this MapPolygon poly, MapPolygon p, Data data)
    {
        var w = data.Planet.Width;
        var off1 = p.Center - poly.Center;
        var off2 = (off1 + Vector2.Right * w);
        var off3 = (off1 + Vector2.Left * w);
        if (off1.Length() < off2.Length() && off1.Length() < off3.Length()) return off1;
        if (off2.Length() < off1.Length() && off2.Length() < off3.Length()) return off2;
        return off3;
    }

    public static MapChunk GetChunk(this MapPolygon poly, Data data)
    {
        return data.Planet.PolygonAux.ChunksByPoly[poly];
    }
    public static Vector2 GetOffsetTo(this MapPolygon poly, Vector2 p, Data data)
    {
        var w = data.Planet.Width;
        var off1 = p - poly.Center;
        var off2 = (off1 + Vector2.Right * w);
        var off3 = (off1 + Vector2.Left * w);
        if (off1.Length() < off2.Length() && off1.Length() < off3.Length()) return off1;
        if (off2.Length() < off1.Length() && off2.Length() < off3.Length()) return off2;
        return off3;
    }
    public static float GetScore(this MapPolygon poly, MapPolygon closest, MapPolygon secondClosest, 
        Vector2 pRel, Data data, Func<MapPolygon, float> getScore)
    {
        var l = pRel.Length();
        var closeL = (poly.GetOffsetTo(closest, data) - pRel).Length();
        var secondCloseL = (poly.GetOffsetTo(secondClosest, data) - pRel).Length();
        var totalDist = l + closeL + secondCloseL;
        var closeInt = Mathf.Lerp(getScore(poly), getScore(closest), closeL / (l + closeL));
        var secondInt = Mathf.Lerp(getScore(poly), getScore(secondClosest), secondCloseL / (l + secondCloseL));

        return (closeInt + secondInt) / 2f;
    }
    public static bool HasNeighbor(this MapPolygon poly, MapPolygon n) => poly.Neighbors.RefIds.Contains(n.Id);
    public static bool IsWater(this MapPolygon poly) => poly.IsLand == false;
    public static bool IsCoast(this MapPolygon poly, Data data) => poly.IsLand && poly.Neighbors.Items(data).Any(n => n.IsWater());
    public static MapPolygonEdge GetEdge(this MapPolygon poly, MapPolygon neighbor, Data data) 
        => data.Planet.PolyEdgeAux.GetEdge(poly, neighbor);
    public static PolyBorderChain GetBorder(this MapPolygon poly, int nId) => poly.NeighborBorders[nId];
    public static IEnumerable<PolyBorderChain> GetPolyBorders(this MapPolygon poly) => poly.Neighbors.RefIds
        .Select(n => poly.GetBorder(n));
    public static List<LineSegment> GetOrderedBoundarySegs(this MapPolygon poly, Data data)
    {
        return data.Planet
            .PolygonAux
            .AuxDatas[poly]
            .OrderedBoundarySegs;
    }
    public static Vector2[] GetOrderedBoundaryPoints(this MapPolygon poly, Data data)
    {
        return data.Planet.PolygonAux.AuxDatas[poly].OrderedBoundaryPoints;
    }
    public static ReadOnlyHash<ResourceDeposit> GetResourceDeposits(this MapPolygon p, Data data)
    {
        var rd = data.Planet.ResourceDepositAux.ByPoly[p];
        return rd == null ? null : new ReadOnlyHash<ResourceDeposit>(rd.ToHashSet());
    }

    public static List<MapBuilding> GetBuildings(this MapPolygon poly, Data data)
    {
        return data.Infrastructure.BuildingAux.ByPoly[poly];
    }
    public static int GetLaborSurplus(this MapPolygon poly, Data data)
    {
        if (poly.GetBuildings(data) == null) return 0;
        if (poly.GetPeep(data) == null) return 0;
        
        return data.Infrastructure.BuildingAux.ByPoly[poly]
            .Select(b => b.Model.Model(data))
            .SelectWhere(b => b.HasComponent<Workplace>())
            .Sum(wb => poly.GetPeep(data).Size - wb.GetComponent<Workplace>().TotalLaborReq());
    }
    public static Vector2 GetGraphicalCenterOffset(this MapPolygon poly, Data data)
    {
        return data.Planet.PolygonAux.AuxDatas[poly].GraphicalCenter;
    }

    public static bool HasSettlement(this MapPolygon p, Data data)
    {
        return GetSettlement(p, data) != null;
    }
    public static Settlement GetSettlement(this MapPolygon p, Data data)
    {
        return data.Infrastructure.SettlementAux.ByPoly.ContainsKey(p) ? data.Infrastructure.SettlementAux.ByPoly[p] : null;
    }
    public static Peep GetPeep(this MapPolygon poly, Data data)
    {
        return data.Society.PolyPeepAux.ByPoly[poly];
    }
    public static bool HasPeep(this MapPolygon poly, Data data)
    {
        return data.Society.PolyPeepAux.ByPoly[poly] != null;
    }

    public static IEnumerable<MapPolyNexus> GetNexi(this MapPolygon p, Data data)
    {
        var edges = p.GetEdges(data);
        return edges.Select(e => e.HiNexus.Entity(data)).Union(edges.Select(e => e.LoNexus.Entity(data))).Distinct();
    }
    public static IEnumerable<MapPolygonEdge> GetEdges(this MapPolygon p, Data data)
    {
        return p.Neighbors.Items(data).Select(n => p.GetEdge(n, data));
    }

    public static List<Construction> GetCurrentConstructions(this MapPolygon poly, Data data)
    {
        var curr = data.Infrastructure.CurrentConstruction.ByPoly;
        if (curr.ContainsKey(poly.Id)) return curr[poly.Id];
        return null;
    }

    public static IEnumerable<MapChunk> GetChunkAndNeighboringChunks(this MapPolygon p, Data d)
    {
        return p.GetChunk(d).Yield()
            .Union(p.Neighbors.Items(d).Select(n => n.GetChunk(d))).Distinct();
    }

    public static bool LineEntersPoly(this MapPolygon poly, Vector2 aRel, Vector2 bRel, Data data)
    {
        return poly.GetOrderedBoundaryPoints(data)
            .Any(p => Vector2Ext.LineSegmentsIntersectInclusive(Vector2.Zero, p, aRel, bRel));
    }
}