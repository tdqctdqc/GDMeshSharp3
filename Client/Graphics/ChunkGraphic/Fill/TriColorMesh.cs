using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract partial class TriColorMesh<TElement> 
    : MeshInstance2D, IChunkGraphicModule
{
    public string Name { get; private set; }
    public Node2D Node => this;
    private Dictionary<TElement, int> _elementTriCounts;
    private Vector2[] _vertices;
    private IReadOnlyList<TElement> _elements;
    private List<Color> _colors;
    private ArrayMesh _arrayMesh;
    public TriColorMesh(string name, 
        Vector2 mapPos,
        LayerOrder layerOrder,
        Dictionary<TElement, int> elementTriCounts,
        IReadOnlyList<TElement> elements,
        Vector2[] vertices,
        Data data)
    {
        ZAsRelative = false;
        ZIndex = (int)layerOrder;
        Name = name;
        _vertices = vertices;
        _elementTriCounts = elementTriCounts;
        _elements = elements;
        _colors = new List<Color>();
    }
    public abstract Color GetColor(TElement cell, Data d);
    public abstract void RegisterForRedraws(Data d);
    public void Draw(Data d)
    {
        _colors.Clear();
        for (var i = 0; i < _elements.Count; i++)
        {
            var e = _elements[i];
            var color = GetColor(e, d);
            var triCount = _elementTriCounts[e];
            for (var j = 0; j < triCount; j++)
            {
                _colors.Add(color);
            }
        }        
        if (_vertices.Length < 3) _arrayMesh = new ArrayMesh();
        else _arrayMesh = MeshGenerator.GetArrayMesh(_vertices, 
            _colors.ToArray());
        Mesh = _arrayMesh;
    }
}
