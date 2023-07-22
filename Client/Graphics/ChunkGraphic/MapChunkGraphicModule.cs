using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MapChunkGraphicModule : Node2D, IMapChunkGraphicNode
{
    public bool Hidden { get; set; }
    public string Name { get; private set; }
    private List<IMapChunkGraphicNode> _nodes;
    public MapChunkGraphicModule(string name)
    {
        Hidden = false;
        Name = name;
        _nodes = new List<IMapChunkGraphicNode>();
    }

    private MapChunkGraphicModule()
    {
    }

    public void AddLayer(IMapChunkGraphicNode layer)
    {
        AddChild((Node)layer);
        _nodes.Add(layer);
    }

    public void Init(Data data)
    {
        foreach (var l in _nodes)
        {
            l.Init(data);
        }
    }
    public void UpdateVis(Data data)
    {
        if (Hidden)
        {
            Visible = false;
            return;
        }
        else Visible = true;
        var scaledZoom = Game.I.Client.Cam.ScaledZoomOut;
        foreach (var node in _nodes)
        {
            node.UpdateVis(data);
        }
    }

    Node2D IMapChunkGraphicNode.Node => this;
}
