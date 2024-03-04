using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
        var sw = new Stopwatch();
        sw.Start();
        Chunks = data.Planet.PolygonAux.Chunks
            // .AsParallel()
            .Select(c =>
            {
                var graphic = new ChunkGraphicHolder(c, data);
                graphic.Draw(data);
                return (c, graphic);
            })
            .ToDictionary(c => c.c,
                c => c.Item2);
        
        foreach (var kvp in Chunks)
        {
            segmenter.AddElement(kvp.Value, kvp.Key.RelTo.Center);
        }
        sw.Stop();
        GD.Print("Make graphic chunks " + sw.Elapsed.TotalMilliseconds);
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

