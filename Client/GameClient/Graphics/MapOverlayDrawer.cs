
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
        var mb = MeshBuilder.GetFromPool();
        draw(mb);
        var mi = mb.GetMeshInstance();
        AddNode(mi, pos);
        mb.Return();
    }

    public void Label(string text, Color color, Vector2 pos)
    {
        var label = NodeExt.CreateLabel(text);
        label.Modulate = color;
        var node = new Node2D();
        node.AddChild(label);
        AddNode(node, pos);
    }

    private void AddNode(Node2D node, Vector2 pos)
    {
        node.ZIndex = _z;
        _nodes.Add(node);
        _segmenter.AddElement(node, pos);
    }
}