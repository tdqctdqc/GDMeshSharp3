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
        var mb = MeshBuilder.GetFromPool();
        DrawRoads(chunk, d, mb);
        if (mb.Tris.Count == 0) return;
        mb.Return();
    }

    public void DrawRoads(MapChunk chunk, Data d, 
        MeshBuilder mb)
    {
        this.ClearChildren();
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
        AddChild(mb.GetMeshInstance());
    }
    public static ChunkGraphicLayer<RoadChunkGraphicNode> 
        GetLayer(Client client, GraphicsSegmenter segmenter)
    {
        
        var l = new ChunkGraphicLayer<RoadChunkGraphicNode>(LayerOrder.Roads,
            "Roads", segmenter, 
            c => new RoadChunkGraphicNode(c, client.Data), client.Data);
        
        client.Data.Notices.Ticked.Blank.Subscribe(() =>
        {
            client.QueuedUpdates.Enqueue(() =>
            {
                var mb = MeshBuilder.GetFromPool();
                foreach (var kvp in l.ByChunkCoords)
                {
                    kvp.Value.DrawRoads(kvp.Value.Chunk, client.Data, mb);
                }
                mb.Return();
            });
        });
        
        return l;
    }
}
