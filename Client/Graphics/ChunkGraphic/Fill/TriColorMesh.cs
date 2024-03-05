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
    private Color[] _colors;
    private ArrayMesh _arrayMesh;
    public ChunkGraphicModuleVisibility Visibility { get; }

    public TriColorMesh(string name, 
        Vector2 mapPos,
        LayerOrder layerOrder,
        Dictionary<TElement, int> elementTriCounts,
        IReadOnlyList<TElement> elements,
        Vector2[] vertices,
        Vector2 visibleZoomRange,
        Data data)
    {
        Visibility = new ChunkGraphicModuleVisibility(visibleZoomRange);
        ZIndex = (int)layerOrder;
        Name = name;
        _vertices = vertices;
        _elementTriCounts = elementTriCounts;
        _elements = elements;
        _colors = new Color[_elementTriCounts.Sum(kvp => kvp.Value) * 3];
    }
    public abstract Color GetColor(TElement cell, Data d);
    public abstract void RegisterForRedraws(Data d);
    public abstract Settings GetSettings(Data d);

    public void Draw(Data d)
    {
        int iter = 0;
        for (var i = 0; i < _elements.Count; i++)
        {
            var e = _elements[i];
            var color = GetColor(e, d);
            var triCount = _elementTriCounts[e];
            for (var j = 0; j < triCount; j++)
            {
                _colors[iter] = color;
                iter++;
                _colors[iter] = color;
                iter++;
                _colors[iter] = color;
                iter++;
            }
        }        
        if (_vertices.Length < 3) _arrayMesh = new ArrayMesh();
        else _arrayMesh = MeshGenerator.GetArrayMesh(_vertices, 
            _colors);
        Mesh = _arrayMesh;
    }
}
