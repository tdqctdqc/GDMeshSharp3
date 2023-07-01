using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MapChunkGraphicModule : Node2D
{
    private Dictionary<IMapChunkGraphicLayer, Vector2> _layers;
    public MapChunkGraphicModule()
    {
        _layers = new Dictionary<IMapChunkGraphicLayer, Vector2>();
    }

    protected void AddLayer(Vector2 range, IMapChunkGraphicLayer layer)
    {
        AddChild((Node)layer);
        _layers.Add(layer, range);
    }
    public void Update(Data data)
    {
        // var zoom = Game.I.Client.Cam.ZoomOut;
        var scaledZoom = Game.I.Client.Cam.ScaledZoomOut;
        foreach (var kvp in _layers)
        {
            var range = kvp.Value;
            if (scaledZoom >= range.X && scaledZoom <= range.Y)
            {
                kvp.Key.Node.Visible = true;
                kvp.Key.Update(data);
            }
            else
            {
                kvp.Key.Node.Visible = false;
            }
        }
    }
}
