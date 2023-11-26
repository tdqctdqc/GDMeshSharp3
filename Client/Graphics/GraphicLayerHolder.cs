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
        AddLayer(Terrain(0, segmenter, data), true);
        AddLayer(PolyFill(1, segmenter, data), true);
        AddLayer(Roads(2, segmenter, data), true);
        AddLayer(IconsChunkModule.GetLayer(3, data, segmenter), true);
        AddLayer(ResourceChunkModule.GetLayer(4, data, segmenter), false);
        AddLayer(FrontGraphicLayer.GetLayer(5, client, segmenter, data), true);
        AddLayer(WaypointGraphicChunk.GetLayer(6, "Nav Waypoints", data, client, 
            p => p.GetAssocNavWaypoints(data), wp => wp.GetNeighboringNavWaypoints(data), segmenter), false);
        AddLayer(WaypointGraphicChunk.GetLayer(7, "Tac Waypoints", data, client, 
            p => p.GetAssocTacWaypoints(data), wp => wp.TacNeighbors(data), segmenter), false);
        AddLayer(UnitOrdersGraphicLayer.GetLayer(8, segmenter, client), true);
        AddLayer(new UnitGraphicLayer(9, client, segmenter, data), true);
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
        d.Logger.Log($"graphics update time {sw.Elapsed.TotalMilliseconds}",
            LogType.Graphics);
    }

    private ChunkGraphicSwitchLayer PolyFill(int z, GraphicsSegmenter segmenter, Data data)
    {
        var regime = RegimeChunkModule.GetLayer(z, data, segmenter);
        var diplomacy = DiplomacyChunkModule.GetLayer(z, data, segmenter);
        var alliance = AllianceChunkModule.GetLayer(z, data, segmenter);
        return new ChunkGraphicSwitchLayer(z, "Poly Fill", regime, diplomacy, alliance);
    }
    private ChunkGraphicLayer<RoadChunkGraphicNode> Roads(int z, GraphicsSegmenter segmenter, 
        Data d)
    {
        var l = new ChunkGraphicLayer<RoadChunkGraphicNode>(z, "Roads", segmenter,
            c => new RoadChunkGraphicNode(c, d),
            d);
        return l;
    }
    private ChunkGraphicLayer<TerrainChunkModule> Terrain(int z, GraphicsSegmenter segmenter, Data d)
    {
        var l = new ChunkGraphicLayer<TerrainChunkModule>(z, "Terrain", segmenter,
            c => new TerrainChunkModule(c, d),
            d);
        return l;
    }
    private ChunkGraphicLayer<PolyFillChunkGraphic> ResourceDepositPolyFill(int z, GraphicsSegmenter segmenter, 
        Data data, MapGraphics mg)
    {
        var l = new ChunkGraphicLayer<PolyFillChunkGraphic>(z, "Resources", segmenter,
            c => new PolyFillChunkGraphic(nameof(ResourceDepositPolyFill), c, (p, d) =>
            {
                var rs = p.GetResourceDeposits(d);
                if (rs == null || rs.Count == 0) return new Color(Colors.Pink, .5f);
                return rs.First().Item.Model(d).Color;
            }, data), data);
        return l;
    }
}
