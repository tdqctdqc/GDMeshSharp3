using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GeometryExceptionDisplay : Node2D
{
    private int _iter = 0;
    
    private static float _widthStep = .3f,
        _markerSize = 1f;
    private static Vector2 _buffer = Vector2.Down * 100f,
        _pos = Vector2.Zero;
    
    public void Setup(GeometryException e)
    {
        var message = new Label();
        message.Text = e.Message;
        message.Scale = Vector2.One * 2f;
        message.Position = -_buffer * 2f;
        AddChild(message);
        
        SetupSegs(e);
        SetupPoints(e);
        SetupTris(e);
    }

    private void SetupSegs(GeometryException e)
    {
        for (int i = 0; i < e.SegLayers.Count; i++)
        {
            _iter++;
            var mb = new MeshBuilder();
            mb.AddCircle(Vector2.Zero, 10f, 12, Colors.Pink);
            var segs = e.SegLayers[i];
            
            var height = GetRange(segs.GetPoints().Select(s => s.Y));
            var width = GetRange(segs.GetPoints().Select(s => s.X));

            var col = ColorsExt.GetRainbowColor(_iter);
            mb.AddArrows(segs, (e.SegLayers.Count - _iter + 1) * _widthStep, col);
            mb.AddNumMarkers(segs.Select(s => s.Mid()).ToList(), 
                _markerSize, Colors.Transparent, Colors.White, Vector2.Zero);
            
            AddLabel(e.SegLayerNames[i], col);

            var segsScroll = MakeScroll(segs.Select(s => s.ToString()),
                new Vector2(500f, Mathf.Max(height, 100f)), width);
            
            for (var j = 0; j < segs.Count; j++)
            {
                var seg = segs[j];
                if (seg.To == segs.Modulo(j + 1).From)
                {
                    mb.AddSquare(seg.To, _markerSize, Colors.White);
                }
                else
                {
                    mb.AddSquare(seg.To, _markerSize, Colors.Black);
                    mb.AddSquare(segs.Modulo(j + 1).From, _markerSize, Colors.Red);
                }
            }
            var child = mb.GetMeshInstance();
            child.Position = _pos;
            AddChild(child);
            MovePos(height);
        }
    }

    private void SetupPoints(GeometryException e)
    {
        for (var i = 0; i < e.PointSets.Count; i++)
        {
            var mb = new MeshBuilder();
            _iter++;
            var points = e.PointSets[i];
            var col = ColorsExt.GetRainbowColor(_iter);
            mb.AddPointMarkers(points, _markerSize, col);
            var width = GetRange(points.Select(p => p.X));
            var height = GetRange(points.Select(p => p.Y));

            var pointsScroll = MakeScroll(points.Select(p => p.ToString()), 
                new Vector2(500f, Mathf.Max(height, 100f)), width);
            
            mb.AddNumMarkers(points, _markerSize, Colors.Transparent, Colors.White,
                Vector2.Zero);
            
            AddLabel("Points " + e.PointSetNames[i], col);
            
            var child = mb.GetMeshInstance();
            child.Position = _pos;
            AddChild(child);
            
            MovePos(height);
        }
    }
    private void SetupTris(GeometryException e)
    {
        for (var i = 0; i < e.TriSets.Count; i++)
        {
            var mb = new MeshBuilder();
            _iter++;
            var col = ColorsExt.GetRainbowColor(_iter);

            var tris = e.TriSets[i];
            if (tris.Count == 0) continue;
            tris.ForEach(t => mb.AddTri(t, t.Color));
            var width = GetRange(tris.SelectMany(t => new List<float>(){t.A.X, t.B.X, t.C.X}));
            var height = GetRange(tris.SelectMany(t => new List<float>(){t.A.Y, t.B.Y, t.C.Y}));

            var triScroll = MakeScroll(tris.Select(p => p.ToString()), 
                new Vector2(500f, Mathf.Max(height, 100f)), width);
            
            mb.AddNumMarkers(tris.Select(t => t.GetCentroid()).ToList(), 
                _markerSize, Colors.Transparent, Colors.White,
                Vector2.Zero);
            
            AddLabel("Tris " + e.TriSetNames[i], col);
            
            var child = mb.GetMeshInstance();
            child.Position = _pos;
            AddChild(child);
            
            MovePos(height);
        }
    }
    private void MovePos(float shift)
    {
        _pos += shift * Vector2.Down;
        _pos += _buffer;
    }

    private void AddLabel(string text, Color col)
    {
        var label = new Label();
        label.Text = text;
        label.SelfModulate = col;
        AddChild(label);
        label.Position = _pos;
        label.Position -= _buffer / 2f;
    }
    private float GetRange(IEnumerable<float> nums)
    {
        var max = nums.Max();
        var min = nums.Min();
        var range = max - min;
        range = new List<float> {max, min, range}.Max();
        return range;
    }
    private ScrollContainer MakeScroll(IEnumerable<string> texts, Vector2 size, float width)
    {
        var scroll = new ScrollContainer();
        scroll.Size = size;
        var box = new VBoxContainer();
        scroll.AddChild(box);
        int iter = 0;
        foreach (var text in texts)
        {
            box.AddChild(NodeExt.CreateLabel(iter++ + " " + text));
        }
        scroll.Position = _pos + width * Vector2.Right;
        AddChild(scroll);
        return scroll;
    }
    
}