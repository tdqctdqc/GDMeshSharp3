
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class BorderChunkNode : MapChunkGraphicNode<MapPolygon>
{
    private float _thickness;
    private Func<MapPolygon, Color> _getColor;
    private Func<MapPolygon, MapPolygon, bool> _inUnion;
    public BorderChunkNode(string name, MapChunk chunk, Func<MapPolygon, MapPolygon, bool> inUnion, 
        Func<MapPolygon, Color> getColor, float thickness, Data data)
        : base(name, data, chunk)
    {
        _getColor = getColor;
        _inUnion = inUnion;
        _thickness = thickness;
        Init(data);
    }
    private BorderChunkNode() : base()
    {
    }
    protected override Node2D MakeGraphic(MapPolygon element, Data data)
    {
        var mb = new MeshBuilder();
        var color = _getColor(element);
        var offset = Chunk.RelTo.GetOffsetTo(element, data);
        foreach (var n in element.Neighbors.Items(data))
        {
            if (_inUnion(n, element)) continue;
            mb.DrawMapPolyEdge(element, n, data, _thickness, color, offset);
        }

        if (mb.Tris.Count == 0) return new Node2D();
        return mb.GetMeshInstance();
    }

    protected override IEnumerable<MapPolygon> GetKeys(Data data)
    {
        return Chunk.Polys
            .Where(p => p.Regime.Empty() == false 
                        && p.Neighbors.Items(data).Any(n => _inUnion(n, p) == false));
    }

    protected override bool Ignore(MapPolygon element, Data data)
    {
        return element.Regime.Fulfilled() == false
               || element.Neighbors.Items(data).Any(n => n.Regime.RefId != element.Regime.RefId) == false;
    }

    public void QueueChangeAround(MapPolygon poly, Data data)
    {
        QueueChange(poly);
        foreach (var n in poly.Neighbors.Items(data))
        {
            QueueChange(n);
        }
    }
}