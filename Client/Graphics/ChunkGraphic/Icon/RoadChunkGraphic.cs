using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RoadChunkGraphicNode : MapChunkGraphicModule
{
    private static float _drawWidth = 5f;
    
    public RoadChunkGraphicNode(MapChunk chunk, Data d) 
        : base(chunk, nameof(RoadChunkGraphicNode))
    {
        var mb = new MeshBuilder();
        DrawRoads(chunk, d, mb);
        if (mb.Tris.Count == 0) return;
        AddChild(mb.GetMeshInstance());
    }

    private static void DrawRoads(MapChunk chunk, Data d, MeshBuilder mb)
    {
        var wps = chunk.Polys.SelectMany(p => d.Military.TacticalWaypoints.PolyAssocWaypoints[p.Id])
            .Distinct();
        foreach (var id in wps)
        {
            var wp = MilitaryDomain.GetWaypoint(id, d);
            foreach (var n in wp.Neighbors)
            {
                if (n > wp.Id) continue;
                var nWp = MilitaryDomain.GetWaypoint(n, d);
                if (d.Infrastructure.RoadNetwork.Get(wp, nWp, d) is RoadModel r)
                {
                    r.Draw(mb, chunk.RelTo.GetOffsetTo(wp.Pos, d), 
                        chunk.RelTo.GetOffsetTo(nWp.Pos, d), _drawWidth);
                }
            }
        }
        var edges = d.Infrastructure.RoadNetwork.Roads.Dic.Keys
            .Where(k => chunk.Polys.Contains(d[(int)k.X]));
        
    }
    public static ChunkGraphicLayer<RoadChunkGraphicNode> GetLayer(Data d, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<RoadChunkGraphicNode>(LayerOrder.Roads,
            "Roads", segmenter, 
            c => new RoadChunkGraphicNode(c, d), d);
        return l;
    }
}
