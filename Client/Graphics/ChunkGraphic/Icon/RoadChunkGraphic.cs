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
        var nav = d.Planet.NavWaypoints;
        var mb = new MeshBuilder();
        DrawRoads(chunk, d, nav, mb);
        if (mb.Tris.Count == 0) return;
        AddChild(mb.GetMeshInstance());
    }

    private static void DrawRoads(MapChunk chunk, Data d, NavWaypoints navWaypoints, MeshBuilder mb)
    {
        var wps = chunk.Polys.SelectMany(p => d.Planet.NavWaypoints.GetPolyAssocWaypoints(p, d))
            .Distinct();
        foreach (var wp in wps)
        {
            foreach (var n in wp.Neighbors)
            {
                if (n > wp.Id) continue;
                var nWp = navWaypoints.Get(n);
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
    public static ChunkGraphicLayer<RoadChunkGraphicNode> GetLayer(int z, Data d, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<RoadChunkGraphicNode>(z,
            "Roads", segmenter, 
            c => new RoadChunkGraphicNode(c, d), d);
        return l;
    }
}
