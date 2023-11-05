
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract partial class BorderChunkNode 
    : MapChunkGraphicNode<MapPolygon>
{
    public BorderChunkNode(string name, MapChunk chunk, 
        Data data)
        : base(name, data, chunk)
    {
    }
    private BorderChunkNode() : base()
    {
    }

    protected abstract bool InUnion(MapPolygon p1, MapPolygon p2, Data data);
    protected abstract float GetThickness(MapPolygon p1, MapPolygon p2, Data data);
    protected abstract Color GetColor(MapPolygon p1, Data data);
    
    protected override Node2D MakeGraphic(MapPolygon element, Data data)
    {
        var mb = new MeshBuilder();
        var color = GetColor(element, data);
        var offset = Chunk.RelTo.GetOffsetTo(element, data);
        foreach (var n in element.Neighbors.Items(data))
        {
            if (InUnion(n, element, data)) continue;
            mb.DrawPolyEdge(element, n, p => GetColor(p, data), GetThickness(element, n, data), Chunk.RelTo, data);
        }
        
        if (mb.Tris.Count == 0) return new Node2D();
        return mb.GetMeshInstance();
    }

    protected override IEnumerable<MapPolygon> GetKeys(Data data)
    {
        return Chunk.Polys
            .Where(p =>
            {
                var ns = p.Neighbors.Items(data);
                var r = p.Regime;
                var rFulfilled = p.Regime.Fulfilled();
                var nNotInUnion = ns.Any(n => InUnion(n, p, data) == false);
                return rFulfilled
                       && nNotInUnion;
            });
    }

    protected override bool Ignore(MapPolygon element, Data data)
    {
        return element.Regime.Fulfilled() == false
               || element.Neighbors.Items(data).Any(n => n.Regime.RefId != element.Regime.RefId) == false;
    }

    public void QueueChangeAround(MapPolygon poly, Data data)
    {
        if(Chunk.Polys.Contains(poly)) QueueChange(poly);
        foreach (var n in poly.Neighbors.Items(data).Where(n => Chunk.Polys.Contains(n)))
        {
            if (Ignore(n, data)) continue;
            QueueChange(n);
        }
    }
}