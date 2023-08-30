using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RoadChunkGraphicNode : MapChunkGraphicModule
{
    
    public RoadChunkGraphicNode(MapChunk chunk, Data d) 
        : base(chunk, nameof(RoadChunkGraphicNode))
    {
        var nav = d.Planet.Nav;
        var mb = new MeshBuilder();
        DrawRoads(chunk, d, nav, mb);
        if (mb.Tris.Count == 0) return;
        AddChild(mb.GetMeshInstance());
    }

    private static void DrawRoads(MapChunk chunk, Data d, Nav nav, MeshBuilder mb)
    {
        var wps = chunk.Polys.SelectMany(p => d.Planet.Nav.GetPolyAssocWaypoints(p, d))
            .Distinct();
        foreach (var wp in wps)
        {
            foreach (var n in wp.Neighbors)
            {
                if (n > wp.Id) continue;
                var nWp = nav.Get(n);
                if (d.Infrastructure.RoadNetwork.Get(wp, nWp, d) is RoadModel r)
                {
                    r.Draw(mb, chunk.RelTo.GetOffsetTo(wp.Pos, d), 
                        chunk.RelTo.GetOffsetTo(nWp.Pos, d), 10f);
                }
            }
        }
        var edges = d.Infrastructure.RoadNetwork.Roads.Dic.Keys
            .Where(k => chunk.Polys.Contains(d[(int)k.X]));
        
    }
    public static ChunkGraphicLayer<RoadChunkGraphicNode> GetLayer(Data d, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<RoadChunkGraphicNode>("Roads", segmenter, 
            c => new RoadChunkGraphicNode(c, d), d);
        return l;
    }
}
