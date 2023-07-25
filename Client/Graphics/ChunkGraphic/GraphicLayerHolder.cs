using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class GraphicLayerHolder
{
    public List<IGraphicLayer> Layers { get; private set; }
    public GraphicLayerHolder(GraphicsSegmenter segmenter, MapGraphics mg, Data data)
    {
        Layers = new List<IGraphicLayer>();
        Layers.Add(Terrain(segmenter, data, mg));
        Layers.Add(PolyFill(segmenter, data, mg));
        // Layers.Add(RegimeChunkModule.GetLayer(data, segmenter));
        // Layers.Add(AllianceChunkModule.GetLayer(data, segmenter));
        // Layers.Add(ResourceDepositPolyFill(segmenter, data, mg));
        Layers.Add(Roads(segmenter, data, mg));
        Layers.Add(IconsChunkModule.GetLayer(data, segmenter));
    }

    public void Update(Data d)
    {
        var sw = new Stopwatch();
        sw.Start();
        foreach (var kvp in Layers)
        {
            kvp.Update(d);
        }
        sw.Stop();
        Game.I.Logger.Log($"graphics update time {sw.Elapsed.TotalMilliseconds}",
            LogType.Graphics);
    }

    private ChunkGraphicSwitchLayer PolyFill(GraphicsSegmenter segmenter, 
        Data data, MapGraphics mg)
    {
        var regime = RegimeChunkModule.GetLayer(data, segmenter);
        var alliance = AllianceChunkModule.GetLayer(data, segmenter);
        return new ChunkGraphicSwitchLayer("Poly Fill", regime, alliance);
    }
    private ChunkGraphicLayer<RoadChunkGraphicNode> Roads(GraphicsSegmenter segmenter, 
        Data d, MapGraphics mg)
    {
        var l = new ChunkGraphicLayer<RoadChunkGraphicNode>("Roads", segmenter,
            c => new RoadChunkGraphicNode(c, d, mg),
            d);
        return l;
    }
    private ChunkGraphicLayer<TerrainChunkModule> Terrain(GraphicsSegmenter segmenter, 
        Data d, MapGraphics mg)
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
