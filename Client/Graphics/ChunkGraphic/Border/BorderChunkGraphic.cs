
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class BorderChunkLayer : MapChunkGraphicLayer<int>
{
    private float _thickness;
    private Func<MapPolygon, int> _getMarker;
    private Func<MapPolygon, Color> _getColor;
    public BorderChunkLayer(string name, MapChunk chunk, Func<MapPolygon, int> getMarker, Func<MapPolygon, Color> getColor, float thickness, Data data, MapGraphics mg)
        : base(name, data, chunk, new Vector2(0f, 1f),
            mg.ChunkChangedCache.PolyRegimeChanged)
    {
        _getColor = getColor;
        _getMarker = getMarker;
        _thickness = thickness;
        Init(data);
    }
    private BorderChunkLayer() : base()
    {
    }
    protected override Node2D MakeGraphic(int key, Data data)
    {
        var p = data.Get<MapPolygon>(key);
        var mb = new MeshBuilder();
        var color = _getColor(p);
        var offset = Chunk.RelTo.GetOffsetTo(p, data);
        foreach (var n in p.Neighbors.Entities(data))
        {
            if (_getMarker(n) == _getMarker(p)) continue;
            mb.DrawMapPolyEdge(p, n, data, _thickness, color, offset);
        }

        if (mb.Tris.Count == 0) return new Node2D();
        return mb.GetMeshInstance();
    }

    protected override IEnumerable<int> GetKeys(Data data)
    {
        return Chunk.Polys
            .Where(p => p.Regime.Empty() == false && p.Neighbors.Entities(data).Any(n => n.Regime.RefId != p.Regime.RefId))
            .Select(p => p.Id);
    }
}