
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapOverlayDrawer
{
    private GraphicsSegmenter _segmenter;
    private List<Node> _nodes;
    private int _z;

    private MapOverlayDrawer()
    {
    }

    public MapOverlayDrawer(GraphicsSegmenter segmenter, int z)
    {
        _segmenter = segmenter;
        _nodes = new List<Node>();
        _z = z;
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
        var mb = new MeshBuilder();
        draw(mb);
        var mi = mb.GetMeshInstance();
        mi.ZIndex = _z;
        _nodes.Add(mi);
        _segmenter.AddElement(mi, pos);
    }
}