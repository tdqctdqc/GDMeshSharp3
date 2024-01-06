using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
public enum LayerOrder
{
    Terrain, PolyFill, 
    Roads, Icons, Resources,
    NavWaypoints, TacWaypoints,
    UnitOrders, Units, Theaters
}
public class GraphicLayerHolder
{
    
    public List<IGraphicLayer> Layers { get; private set; }
    public GraphicLayerHolder(Client client, GraphicsSegmenter segmenter, 
        Node2D hook, Data data)
    {
        Layers = new List<IGraphicLayer>();
        AddLayer(Terrain(segmenter, data), true);
        AddLayer(PolyFill(segmenter, data), true);
        AddLayer(Roads(2, segmenter, data), true);
        AddLayer(IconsChunkModule.GetLayer(data, segmenter), true);
        AddLayer(ResourceChunkModule.GetLayer(data, segmenter), false);
        AddLayer(WaypointGraphicChunk.GetLayer(LayerOrder.TacWaypoints, "Tac Waypoints", data, client, 
            p => p.GetAssocTacWaypoints(data), wp => wp.TacNeighbors(data), segmenter), false);
        AddLayer(UnitOrdersGraphicLayer.GetLayer(segmenter, client), true);
        AddLayer(new UnitGraphicLayer(client, segmenter, data), true);
        AddLayer(TheaterGraphicLayer.GetLayer(segmenter, client), false);
    }

    private void AddLayer(IGraphicLayer layer, bool startVisible)
    {
        layer.Visible = startVisible;
        Layers.Add(layer);
    }

    private ChunkGraphicSwitchLayer PolyFill(GraphicsSegmenter segmenter, Data data)
    {
        var regime = RegimeChunkModule.GetLayer(data, segmenter);
        var diplomacy = DiplomacyChunkModule.GetLayer(data, segmenter);
        var alliance = AllianceChunkModule.GetLayer(data, segmenter);
        return new ChunkGraphicSwitchLayer(LayerOrder.PolyFill, "Poly Fill", regime, diplomacy, alliance);
    }
    private ChunkGraphicLayer<RoadChunkGraphicNode> Roads(int z, GraphicsSegmenter segmenter, 
        Data d)
    {
        var l = new ChunkGraphicLayer<RoadChunkGraphicNode>(LayerOrder.Roads, "Roads", segmenter,
            c => new RoadChunkGraphicNode(c, d),
            d);
        return l;
    }
    private ChunkGraphicLayer<TerrainChunkModule> Terrain(GraphicsSegmenter segmenter, Data d)
    {
        var l = new ChunkGraphicLayer<TerrainChunkModule>(LayerOrder.Terrain, "Terrain", segmenter,
            c => new TerrainChunkModule(c, d),
            d);
        return l;
    }
    private ChunkGraphicLayer<PolyFillChunkGraphic> ResourceDepositPolyFill(int z, GraphicsSegmenter segmenter, 
        Data data, MapGraphics mg)
    {
        var l = new ChunkGraphicLayer<PolyFillChunkGraphic>(LayerOrder.Resources, "Resources", segmenter,
            c => new PolyFillChunkGraphic(nameof(ResourceDepositPolyFill), c, (p, d) =>
            {
                var rs = p.GetResourceDeposits(d);
                if (rs == null || rs.Count == 0) return new Color(Colors.Pink, .5f);
                return rs.First().Item.Model(d).Color;
            }, data), data);
        return l;
    }
}

