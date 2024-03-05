using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RoadChunkGraphicNode : Node2D, IChunkGraphicModule
{
    private static float _drawWidth = 5f;
    private Vector2 _zoomVisibilityRange;
    public Node2D Node => this;
    public void RegisterForRedraws(Data d)
    {
        this.RegisterDrawOnTick(d);
    }

    public MapChunk Chunk { get; private set; }
    public RoadChunkGraphicNode(MapChunk chunk, 
        Vector2 zoomVisibilityRange,
        Data d)
    {
        _zoomVisibilityRange = zoomVisibilityRange;
        Chunk = chunk;
        ZIndex = (int)LayerOrder.Roads;
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
        if (mb.TriVertices.Count == 0) return;
        AddChild(mb.GetMeshInstance());
        mb.Return();
    }
    public void DoUiTick(UiTickContext context, Data d)
    {
        var zoom = context.ZoomLevel;
        if (_zoomVisibilityRange.X > zoom || _zoomVisibilityRange.Y < zoom)
        {
            Visible = false;
        }
        else
        {
            Visible = true;
        }
    }
}
