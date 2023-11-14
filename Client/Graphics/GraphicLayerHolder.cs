using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class GraphicLayerHolder
{
    public List<IGraphicLayer> Layers { get; private set; }
    public GraphicLayerHolder(Client client, GraphicsSegmenter segmenter, 
        Node2D hook, Data data)
    {
        Layers = new List<IGraphicLayer>();
        AddLayer(Terrain(segmenter, data), true);
        AddLayer(PolyFill(segmenter, data), true);
        AddLayer(Roads(segmenter, data), true);
        AddLayer(IconsChunkModule.GetLayer(data, segmenter), true);
        AddLayer(ResourceChunkModule.GetLayer(data, segmenter), false);
        AddLayer(FrontGraphicLayer.GetLayer(client, segmenter, data), true);
        AddLayer(WaypointGraphicChunk.GetLayer(data, client, segmenter), false);
        AddLayer(UnitOrdersGraphicLayer.GetLayer(segmenter, client), true);
        AddLayer(new UnitGraphicLayer(client, segmenter, data), true);
    }

    private void AddLayer(IGraphicLayer layer, bool startVisible)
    {
        layer.Visible = startVisible;
        Layers.Add(layer);
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
