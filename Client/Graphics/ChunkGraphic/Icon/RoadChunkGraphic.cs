using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RoadChunkGraphicLayer : MapChunkGraphicLayer<int>
{
    private MeshBuilder _mb;

    private RoadChunkGraphicLayer() { }

    public RoadChunkGraphicLayer(MapChunk chunk, Data data, MapGraphics mg) 
        : base(nameof(RoadChunkGraphicLayer), data, chunk, new Vector2(0f, 1f), mg.ChunkChangedCache.RoadsChanged)
    {
        _mb = new MeshBuilder();
    }

    protected override Node2D MakeGraphic(int key, Data data)
    {
        _mb.Clear();
        var seg = data.Society.RoadSegments[key];
        var hi = seg.Edge.Entity(data).HighPoly.Entity(data);
        var lo = seg.Edge.Entity(data).LowPoly.Entity(data);
        seg.Road.Model(data).Draw(_mb, Chunk.RelTo.GetOffsetTo(hi.Center, data), 
            Chunk.RelTo.GetOffsetTo(lo.Center, data), 10f);
        var mesh = _mb.GetMeshInstance();
        _mb.Clear();
        return mesh;
    }

    protected override IEnumerable<int> GetKeys(Data data)
    {
        var res = new List<int>();
        foreach (var p in Chunk.Polys)
        {
            foreach (var n in p.Neighbors.Entities(data))
            {
                if (p.Id > n.Id)
                {
                    var border = p.GetEdge(n, data);
                    if (data.Society.RoadAux.ByEdgeId.ContainsKey(border.Id))
                    {
                        var seg = data.Society.RoadAux.ByEdgeId[border.Id];
                        res.Add(seg.Id);
                    }
                }
            }
        }

        return res;
    }
}
