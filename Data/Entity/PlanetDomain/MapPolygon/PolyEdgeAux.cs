using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class PolyEdgeAux
{
    private PropEntityIndexer<MapPolygonEdge, Vector2> _byEdge;
    public PolyEdgeAux(Data data)
    {
        _byEdge = PropEntityIndexer<MapPolygonEdge, Vector2>.CreateConstant(data, e => MakeEdge(e, data));
    }
    private Vector2 MakeEdge(MapPolygonEdge e, Data data)
    {
        return e.HighPoly.Entity(data).GetIdEdgeKey(e.LowPoly.Entity(data));
    }
    public MapPolygonEdge GetEdge(MapPolygon p1, MapPolygon p2)
    {
        if (p1.HasNeighbor(p2) == false) throw new Exception();
        if (p2.HasNeighbor(p1) == false) throw new Exception();
        var e = p1.GetIdEdgeKey( p2);
        return _byEdge[e];
    }
}