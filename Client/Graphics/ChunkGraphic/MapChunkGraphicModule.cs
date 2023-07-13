using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MapChunkGraphicModule : Node2D
{
    private Dictionary<IMapChunkGraphicLayer, Vector2> _layerVisRanges;
    public MapChunkGraphicModule()
    {
        _layerVisRanges = new Dictionary<IMapChunkGraphicLayer, Vector2>();
    }

    protected void AddLayer(Vector2 range, IMapChunkGraphicLayer layer)
    {
        AddChild((Node)layer);
        _layerVisRanges.Add(layer, range);
    }

    public void Init(Data data)
    {
        foreach (var l in _layerVisRanges.Keys)
        {
            l.Init(data);
        }
    }
    public void UpdateVis(Data data)
    {
        var scaledZoom = Game.I.Client.Cam.ScaledZoomOut;
        foreach (var kvp in _layerVisRanges)
        {
            var range = kvp.Value;
            if (scaledZoom >= range.X && scaledZoom <= range.Y)
            {
                kvp.Key.Node.Visible = true;
            }
            else
            {
                kvp.Key.Node.Visible = false;
            }
        }
    }
}
