using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
public enum LayerOrder
{
    Terrain, Rivers, PolyFill, 
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
        AddLayer(Rivers(segmenter, data), true);
        AddLayer(PolyFill(client, segmenter), true);
        AddLayer(Roads(segmenter, data), true);
        AddLayer(IconsChunkModule.GetLayer(client, segmenter), true);
        AddLayer(ResourceChunkModule.GetLayer(data, segmenter), false);
        // AddLayer(UnitOrdersGraphicLayer.GetLayer(segmenter, client), true);
        AddLayer(new UnitGraphicLayer(client, segmenter, data), true);
        AddLayer(TheaterGraphicLayer.GetLayer(segmenter, client), false);
    }

    private void AddLayer(IGraphicLayer layer, bool startVisible)
    {
        layer.Visible = startVisible;
        Layers.Add(layer);
    }

    private ChunkGraphicSwitchLayer PolyFill(Client client,
        GraphicsSegmenter segmenter)
    {
        var regime = OwnerRegimeGraphic.GetLayer(client, segmenter);
        var control = ControllerRegimeGraphic.GetLayer(client, segmenter);
        var diplomacy = DiplomacyChunkModule.GetLayer(client, segmenter);
        var alliance = AllianceChunkModule.GetLayer(client, segmenter);
        
        return new ChunkGraphicSwitchLayer(LayerOrder.PolyFill, "Poly Fill", control, regime, diplomacy, alliance);
    }
    private ChunkGraphicLayer<RoadChunkGraphicNode> Roads(GraphicsSegmenter segmenter, 
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
            c => TerrainChunkModule.GetBase(c, d),
            d);
        return l;
    }
    private ChunkGraphicLayer<TerrainChunkModule> Rivers(GraphicsSegmenter segmenter, Data d)
    {
        var l = new ChunkGraphicLayer<TerrainChunkModule>(
            LayerOrder.Rivers, "Rivers", segmenter,
            c => TerrainChunkModule.GetRiver(c, d),
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

