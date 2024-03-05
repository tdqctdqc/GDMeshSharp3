using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RoadChunkGraphicNode : Node2D, IChunkGraphicModule
{
    public string Name => "Roads";
    private static float _drawWidth = 5f;
    private bool _visibleByZoom;
    public ChunkGraphicModuleVisibility Visibility { get; }

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
        Visibility = new ChunkGraphicModuleVisibility(zoomVisibilityRange);
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
        Visibility.CheckVisibleTick(context, d);
    }
    public Settings GetSettings(Data d)
    {
        var settings = new Settings(Name);
        settings.SettingsOptions.Add(
            this.MakeVisibilitySetting(true));
        return settings;
    }

}
