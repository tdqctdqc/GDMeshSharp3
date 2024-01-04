
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MapOverlayDrawer : Node2D
{
    private GraphicsSegmenter _segmenter;
    private List<Node> _nodes;

    private MapOverlayDrawer()
    {
    }

    public MapOverlayDrawer(GraphicsSegmenter segmenter)
    {
        _segmenter = segmenter;
        _nodes = new List<Node>();
    }

    public void Clear()
    {
        foreach (var node in _nodes)
        {
            node.QueueFree();
        }
        _nodes.Clear();
    }

    public void Draw(Action<MeshBuilder> draw, Vector2 pos)
    {
        GD.Print("drawing");
        var mb = new MeshBuilder();
        draw(mb);
        var mi = mb.GetMeshInstance();
        mi.ZIndex = ZIndex;
        _nodes.Add(mi);
        _segmenter.AddElement(mi, pos);
    }
}