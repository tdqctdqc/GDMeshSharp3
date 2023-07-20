
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RegimeBorderChunkLayer : MapChunkGraphicLayer<int>
{
    private float _thickness;

    public RegimeBorderChunkLayer(MapChunk chunk, float thickness, Data data, MapGraphics mg)
        : base(nameof(RegimeBorderChunkLayer), data, chunk, new Vector2(0f, 1f),
            mg.ChunkChangedCache.PolyRegimeChanged)
    {
        _thickness = thickness;
        Init(data);
    }
    private RegimeBorderChunkLayer() : base()
    {
    }
    protected override Node2D MakeGraphic(int key, Data data)
    {
        var p = data.Planet.Polygons[key];
        var mb = new MeshBuilder();
        var color = p.Regime.Entity(data).SecondaryColor;
        var offset = Chunk.RelTo.GetOffsetTo(p, data);
        foreach (var n in p.Neighbors.Entities(data))
        {
            if (n.Regime.RefId == p.Regime.RefId) continue;
            mb.DrawMapPolyEdge(p, n, data, _thickness, color, offset);
        }

        return mb.GetMeshInstance();
    }

    protected override IEnumerable<int> GetKeys(Data data)
    {
        return Chunk.Polys
            .Where(p => p.Regime.Empty() == false && p.Neighbors.Entities(data).Any(n => n.Regime.RefId != p.Regime.RefId))
            .Select(p => p.Id);
    }
}