using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RoadChunkGraphicNode : Node2D, IChunkGraphicModule
{
    private static float _drawWidth = 5f;
    public Node2D Node => this;
    public MapChunk Chunk { get; private set; }
    public RoadChunkGraphicNode(MapChunk chunk, 
        Data d)
    {
        Chunk = chunk;
        ZIndex = (int)LayerOrder.Roads;
        ZAsRelative = false;
    }

    public void Draw(Data d)
    {
        this.ClearChildren();
        var mb = MeshBuilder.GetFromPool();

        var wps = Chunk.Polys
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
                    r.Draw(mb, Chunk.RelTo.GetOffsetTo(cell.GetCenter(), d), 
                        Chunk.RelTo.GetOffsetTo(nCell.GetCenter(), d), _drawWidth);
                }
            }
        }
        if (mb.Tris.Count == 0) return;
        mb.Return();
        AddChild(mb.GetMeshInstance());
    }
}
