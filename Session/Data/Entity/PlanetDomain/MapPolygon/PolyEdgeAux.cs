using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class PolyEdgeAux : EntityAux<MapPolygonEdge>
{
    private Entity1to1PropIndexer<MapPolygonEdge, Vector2> _byEdge;
    public PolyEdgeAux(Domain domain, Data data) : base(domain, data)
    {
        _byEdge = Entity1to1PropIndexer<MapPolygonEdge, Vector2>.CreateConstant(data, MakeEdge);
    }
    private Vector2 MakeEdge(MapPolygonEdge e)
    {
        return e.HighPoly.Entity().GetV2EdgeKey(e.LowPoly.Entity());
    }
    public MapPolygonEdge GetEdge(MapPolygon p1, MapPolygon p2)
    {
        if (p1.HasNeighbor(p2) == false) throw new Exception();
        if (p2.HasNeighbor(p1) == false) throw new Exception();
        var e = p1.GetV2EdgeKey( p2);
        return _byEdge[e];
    }
}