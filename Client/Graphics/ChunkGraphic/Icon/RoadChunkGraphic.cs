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
        var wps = chunk.Polys
            .Where(p => p.IsLand)
            .SelectMany(p => 
                p.GetCells(d))
            .Distinct();
        foreach (var cell in wps)
        {
            foreach (var n in cell.Neighbors)
            {
                if (n > cell.Id) continue;
                var nCell = PlanetDomainExt.GetPolyCell(n, d);
                if (d.Infrastructure.RoadNetwork.Get(cell, nCell, d) is RoadModel r)
                {
                    r.Draw(mb, chunk.RelTo.GetOffsetTo(cell.GetCenter(), d), 
                        chunk.RelTo.GetOffsetTo(nCell.GetCenter(), d), _drawWidth);
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
