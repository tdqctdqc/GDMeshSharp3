using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GraphicsSegmenter : Node2D, IGraphicsSegmenter
{
    private Dictionary<int, HashSet<Node2D>> _segments;
    private Dictionary<int, Node2D> _segmentNodes;
    private int _center;
    private int _numSegments;
    private float _dimX;
    private float _segWidth;
    public GraphicsSegmenter(int numSegments, Data data)
    {
        _dimX = data.Planet.Width;
        _numSegments = numSegments;
        _segments = new Dictionary<int, HashSet<Node2D>>();
        _segmentNodes = new Dictionary<int, Node2D>();
        _segWidth = _dimX / _numSegments;
        for (int i = 0; i < _numSegments; i++)
        {
            _segments.Add(i, new HashSet<Node2D>());
            var node = new Node2D();
            _segmentNodes.Add(i, node);
            AddChild(node);
        }
    }

    public int GetSegmentIndex(Vector2 pos)
    {
        return Mathf.FloorToInt(pos.X / _segWidth) % _segments.Count;
    }
    public void AddElements<T>(List<T> elements, Func<T, Vector2> getGamePos) where T : Node2D
    {
        elements.ForEach(e => AddElement(e, getGamePos(e)));
    }

    public int AddElement<T>(T e, Vector2 gamePos) where T : Node2D
    {
        var segmentIndex = Mathf.FloorToInt(gamePos.X / _segWidth) % _segments.Count;
        e.Position = gamePos - new Vector2(segmentIndex * _segWidth, 0f);
        _segments[segmentIndex].Add(e);
        _segmentNodes[segmentIndex].AddChildDeferred(e);
        return segmentIndex;
    }

    public void RemoveElement<T>(T e, int segmentIndex) where T : Node2D
    {
        _segments[segmentIndex].Remove(e);
        _segmentNodes[segmentIndex].RemoveChildDeferred(e);
    }

    public int SwitchSegments(Node2D node, Vector2 pos, int oldSegIndex)
    {
        RemoveElement(node, oldSegIndex);
        return AddElement(node, pos);
    }
    public void Update(float ratio)
    {
        var vec = Vector2.Up.Rotated(Mathf.Pi * 2f * ratio);
        var dimX = _segWidth * _segments.Count;
        foreach (var kvp in _segmentNodes)
        {
            kvp.Value.Position = GetSegmentXDisplace(kvp.Key, vec);
        }
    }

    private Vector2 GetSegmentXDisplace(int index, Vector2 vec)
    {
        var parent = _segmentNodes[index];
        var thisRatio = (float)index / (float)_segmentNodes.Count;
        var thisVec = Vector2.Up.Rotated(Mathf.Pi * 2f * thisRatio);
        var angle = vec.AngleTo(thisVec);
        var dimX = _segWidth * _segments.Count;
        var displace = dimX * angle / (Mathf.Pi * 2f);
        return new Vector2(displace, 0f);
    }
}