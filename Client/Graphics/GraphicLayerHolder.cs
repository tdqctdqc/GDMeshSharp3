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
    UnitOrders, Units, Theaters
}
public class GraphicLayerHolder
{
    public Dictionary<MapChunk, ChunkGraphicHolder> Chunks { get; private set; }
    public List<IGraphicLayer> Layers { get; private set; }
    public GraphicLayerHolder(Client client, GraphicsSegmenter segmenter, 
        Node2D hook, Data data)
    {
        Chunks = data.Planet.PolygonAux.Chunks
            .ToDictionary(c => c,
                c => new ChunkGraphicHolder(c, segmenter, data));
        Layers = new List<IGraphicLayer>();
        
        // AddLayer(UnitOrdersGraphicLayer.GetLayer(segmenter, client), true);
        AddLayer(new UnitGraphicLayer(client, segmenter, data), true);
        AddLayer(TheaterGraphicLayer.GetLayer(segmenter, client), false);
    }
    private void AddLayer(IGraphicLayer layer, bool startVisible)
    {
        layer.Visible = startVisible;
        Layers.Add(layer);
    }
}

