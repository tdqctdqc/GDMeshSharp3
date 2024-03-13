using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class MapPolygonExt
{
    public static float DistFromEquatorRatio(this MapPolygon p, Data data)
    {
        var mapHeight = data.Planet.Height;
        return Mathf.Abs((.5f * mapHeight - p.Center.Y) / mapHeight);
    }
    public static bool PointInPolyAbs(this MapPolygon poly, Vector2 posAbs, Data data)
    {
        var posRel = poly.GetOffsetTo(posAbs, data);
        return Geometry2D.IsPointInPolygon(posRel, poly.BoundaryPoints);
    }
    public static bool PointInPolyRel(this MapPolygon poly, Vector2 posRel, Data data)
    {
        return Geometry2D.IsPointInPolygon(posRel, poly.BoundaryPoints);
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
    
    public static IEnumerable<ResourceDeposit> GetResourceDeposits(this MapPolygon p, Data data)
    {
        return p.GetCells(data)
            .Where(c => c.HasResourceDeposit(data))
            .Select(c => c.GetResourceDeposit(data));
    }

    public static List<MapBuilding> GetBuildings(this MapPolygon poly, Data data)
    {
        var bAux = data.Infrastructure.BuildingAux;
        return poly.GetCells(data)
            .Where(c => bAux.ByCell.Contains(c))
            .Select(c => bAux.ByCell[c]).ToList();
    }

    public static IEnumerable<MapPolyNexus> GetNexi(this MapPolygon p, Data data)
    {
        var edges = p.GetEdges(data);
        return edges.Select(e => e.HiNexus.Get(data)).Union(edges.Select(e => e.LoNexus.Get(data))).Distinct();
    }
    public static IEnumerable<MapPolygonEdge> GetEdges(this MapPolygon p, Data data)
    {
        return p.Neighbors.Items(data).Select(n => p.GetEdge(n, data));
    }


    public static List<Cell> GetCells(this MapPolygon p, Data d)
    {
        return d.Planet.MapAux.CellsByPoly[p];
    }

    public static float GetArea(this MapPolygon p, Data d)
    {
        var tris = Geometry2D.TriangulatePolygon(p.BoundaryPoints);
        var area = 0f;
        for (var i = 0; i < tris.Length; i+=3)
        {
            var a = p.BoundaryPoints[tris[i]];
            var b = p.BoundaryPoints[tris[i+1]];
            var c = p.BoundaryPoints[tris[i+2]];
            area += TriangleExt.GetApproxArea(a, b, c);
        }
        
        return area;
    }

    public static Triangle[] GetTriangles(this MapPolygon p, Vector2 relTo, Data d)
    {
        var relBoundary = p.BoundaryPoints;
        var tris = Geometry2D.TriangulatePolygon(relBoundary);
        var res = new Triangle[tris.Length / 3];
        for (var i = 0; i < tris.Length; i+=3)
        {
            res[i / 3] = new Triangle(
                relTo.Offset(relBoundary[tris[i]] + p.Center, d) ,
                relTo.Offset(relBoundary[tris[i + 1]] + p.Center, d) ,
                relTo.Offset(relBoundary[tris[i + 2]] + p.Center, d));
        }

        return res;
    }
    
}