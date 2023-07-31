using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RoadChunkGraphicNode : MapChunkGraphicNode<int>
{
    private MeshBuilder _mb;

    private RoadChunkGraphicNode() { }

    public RoadChunkGraphicNode(MapChunk chunk, Data data) 
        : base(nameof(RoadChunkGraphicNode), data, chunk)
    {
        _mb = new MeshBuilder();
    }

    protected override Node2D MakeGraphic(int element, Data data)
    {
        _mb.Clear();
        var seg = data.Get<RoadSegment>(element);
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
            foreach (var n in p.Neighbors.Items(data))
            {
                if (p.Id > n.Id)
                {
                    var border = p.GetEdge(n, data);
                    if (data.Infrastructure.RoadAux.ByEdgeId.ContainsKey(border.Id))
                    {
                        var seg = data.Infrastructure.RoadAux.ByEdgeId[border.Id];
                        res.Add(seg.Id);
                    }
                }
            }
        }

        return res;
    }

    protected override bool Ignore(int element, Data data)
    {
        return false;
    }
}
