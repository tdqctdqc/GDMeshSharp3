using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;
public enum LayerOrder
{
    Terrain, PolyFill, Rivers, 
    Roads, Icons, Resources,
    UnitOrders, Units, Theaters,
    Labels, Debug, Highlighter, Ui
}
public class GraphicLayerHolder
{
    public Dictionary<MapChunk, ChunkGraphic> Chunks { get; private set; }
    public EntityGraphicReservoir<Unit, UnitGraphic> UnitGraphics { get; private set; }
    private Client _client;
    public GraphicLayerHolder(Client client, GraphicsSegmenter segmenter, 
        Data data)
    {
        _client = client;
        UnitGraphics = new EntityGraphicReservoir<Unit, UnitGraphic>(
            u => new UnitGraphic(u, data), 
            data);
        Chunks = data.Planet.PolygonAux.Chunks
            .Select(c =>
            {
                var graphic = new ChunkGraphic(c, this, data);
                graphic.Draw(data);
                return (c, graphic);
            })
            .ToDictionary(c => c.c,
                c => c.Item2);
        
        foreach (var kvp in Chunks)
        {
            segmenter.AddElement(kvp.Value, kvp.Key.RelTo.Center);
        }
        client.UiTick.Subscribe(DoUiTick);
    }

    private void DoUiTick()
    {
        var context = new UiTickContext(_client);
        foreach (var (chunk, graphic) in Chunks)
        {
            graphic.DoUiTick(context, _client.Data);
        }
    }
}

