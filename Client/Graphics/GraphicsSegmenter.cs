using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GraphicsSegmenter : Node2D, IGraphicsSegmenter
{
    private Dictionary<int, List<Node2D>> _segments;
    private Dictionary<int, Node2D> _segmentNodes;
    private int _center;
    private int _numSegments;
    private float _dimX;
    private float _segWidth;
    public GraphicsSegmenter(int numSegments, Data data)
    {
        _dimX = data.Planet.Width;
        _numSegments = numSegments;
        _segments = new Dictionary<int, List<Node2D>>();
        _segmentNodes = new Dictionary<int, Node2D>();
        _segWidth = _dimX / _numSegments;
        for (int i = 0; i < _numSegments; i++)
        {
            _segments.Add(i, new List<Node2D>());
            var node = new Node2D();
            _segmentNodes.Add(i, node);
            AddChild(node);
        }
    }
    
    public void AddElements<T>(List<T> elements, Func<T, Vector2> getGamePos) where T : Node2D
    {
        elements.ForEach(e =>
        {
            var segmentIndex = Mathf.FloorToInt(getGamePos(e).X / _segWidth) % _segments.Count;
            e.Position = getGamePos(e) - new Vector2(segmentIndex * _segWidth, 0f);
            _segments[segmentIndex].Add(e);
            _segmentNodes[segmentIndex].AddChild(e);
        });
    }

    public void AddElement<T>(T e, Vector2 gamePos) where T : Node2D
    {
        var segmentIndex = Mathf.FloorToInt(gamePos.X / _segWidth) % _segments.Count;
        e.Position = gamePos - new Vector2(segmentIndex * _segWidth, 0f);
        _segments[segmentIndex].Add(e);
        _segmentNodes[segmentIndex].AddChild(e);
    }
    public void Update(float ratio)
    {
        var vec = Vector2.Up.Rotated(Mathf.Pi * 2f * ratio);
        var dimX = _segWidth * _segments.Count;
        var center = dimX * ratio;
        for (var i = 0; i < _segmentNodes.Count; i++)
        {
            var keyValuePair = _segmentNodes.ElementAt(i);
            var thisRatio = (float)keyValuePair.Key / (float)_segmentNodes.Count;
            var thisVec = Vector2.Up.Rotated(Mathf.Pi * 2f * thisRatio);
            var angle = vec.AngleTo(thisVec);
            var displace = dimX * angle / (Mathf.Pi * 2f);
            keyValuePair.Value.Position = new Vector2(displace, 0f);
        }
    }
}