using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class PolyEdgeAux
{
    private Indexer<Vector2, MapPolygonEdge> _byEdge;
    public PolyEdgeAux(Data data)
    {
        _byEdge = Indexer.MakeForEntity<Vector2, MapPolygonEdge>
            (e => MakeEdge(e, data), data);
    }
    private Vector2 MakeEdge(MapPolygonEdge e, Data data)
    {
        return e.HighPoly.Get(data).GetIdEdgeKey(e.LowPoly.Get(data));
    }
    public MapPolygonEdge GetEdge(MapPolygon p1, MapPolygon p2)
    {
        if (p1.HasNeighbor(p2) == false) throw new Exception();
        if (p2.HasNeighbor(p1) == false) throw new Exception();
        var e = p1.GetIdEdgeKey( p2);
        return _byEdge[e];
    }
}