using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class GraphicLayerHolder
{
    public List<IGraphicLayer> Layers { get; private set; }
    public GraphicLayerHolder(GraphicsSegmenter segmenter, Node2D hook, Data data)
    {
        Layers = new List<IGraphicLayer>();
        Layers.Add(Terrain(segmenter, data));
        Layers.Add(PolyFill(segmenter, data));
        Layers.Add(Roads(segmenter, data));
        Layers.Add(IconsChunkModule.GetLayer(data, segmenter));
        Layers.Add(ResourceChunkModule.GetLayer(data, segmenter));
        Layers.Add(WaypointTypeGraphicChunk.GetLayer(data, segmenter));
        Layers.Add(WaypointFrontlineGraphicChunk.GetLayer(data, segmenter));
        Layers.Add(new UnitGraphicLayer(segmenter, data));
        Layers.Add(new FrontGraphicLayer(segmenter, data));
    }

    public void Update(Data d, ConcurrentQueue<Action> queue)
    {
        var sw = new Stopwatch();
        sw.Start();
        foreach (var kvp in Layers)
        {
            kvp.Update(d, queue);
        }
        sw.Stop();
        Game.I.Logger.Log($"graphics update time {sw.Elapsed.TotalMilliseconds}",
            LogType.Graphics);
    }

    private ChunkGraphicSwitchLayer PolyFill(GraphicsSegmenter segmenter, Data data)
    {
        var regime = RegimeChunkModule.GetLayer(data, segmenter);
        var diplomacy = DiplomacyChunkModule.GetLayer(data, segmenter);
        var alliance = AllianceChunkModule.GetLayer(data, segmenter);
        return new ChunkGraphicSwitchLayer("Poly Fill", regime, diplomacy, alliance);
    }
    private ChunkGraphicLayer<RoadChunkGraphicNode> Roads(GraphicsSegmenter segmenter, 
        Data d)
    {
        var l = new ChunkGraphicLayer<RoadChunkGraphicNode>("Roads", segmenter,
            c => new RoadChunkGraphicNode(c, d),
            d);
        return l;
    }
    private ChunkGraphicLayer<TerrainChunkModule> Terrain(GraphicsSegmenter segmenter, Data d)
    {
        var l = new ChunkGraphicLayer<TerrainChunkModule>("Terrain", segmenter,
            c => new TerrainChunkModule(c, d),
            d);
        return l;
    }
    private ChunkGraphicLayer<PolyFillChunkGraphic> ResourceDepositPolyFill(GraphicsSegmenter segmenter, 
        Data data, MapGraphics mg)
    {
        var l = new ChunkGraphicLayer<PolyFillChunkGraphic>("Resources", segmenter,
            c => new PolyFillChunkGraphic(nameof(ResourceDepositPolyFill), c, (p, d) =>
            {
                var rs = p.GetResourceDeposits(d);
                if (rs == null || rs.Count == 0) return new Color(Colors.Pink, .5f);
                return rs.First().Item.Model(d).Color;
            }, data), data);
        return l;
    }
}
