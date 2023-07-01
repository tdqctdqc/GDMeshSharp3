using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GraphicsSegmenter<T> : Node2D, IGraphicsSegmenter where T : Node2D
{
    private Dictionary<int, List<T>> _segments;
    private Dictionary<int, Node2D> _segmentNodes;
    private int _center;
    private float _segWidth;
    public GraphicsSegmenter()
    {
        _segments = new Dictionary<int, List<T>>();
        _segmentNodes = new Dictionary<int, Node2D>();
    }
    
    public void Setup(List<T> elements, int numSegments, Func<T, Vector2> tPos, Data data)
    {
        var dimX = data.Planet.Width;
        _segWidth = dimX / numSegments;
        for (int i = 0; i < numSegments; i++)
        {
            _segments.Add(i, new List<T>());
            var node = new Node2D();
            _segmentNodes.Add(i, node);
            AddChild(node);
        }
        elements.ForEach(e =>
        {
            var segmentIndex = Mathf.FloorToInt(tPos(e).X / _segWidth) % _segments.Count;
            e.Position = tPos(e) - new Vector2(segmentIndex * _segWidth, 0f);
            _segments[segmentIndex].Add(e);
            _segmentNodes[segmentIndex].AddChild(e);
        });
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