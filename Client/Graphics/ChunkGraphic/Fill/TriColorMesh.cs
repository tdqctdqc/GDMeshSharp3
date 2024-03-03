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
    private Dictionary<TElement, List<int>> _triIndicesByElement;
    private List<Vector2> _vertices;
    private List<Color> _colors;
    
    private ArrayMesh _arrayMesh;
    public TriColorMesh(string name, 
        Vector2 mapPos,
        LayerOrder layerOrder,
        GraphicsSegmenter segmenter,
        Data data)
    {
        ZAsRelative = false;
        ZIndex = (int)layerOrder;
        Name = name;
        _triIndicesByElement = new Dictionary<TElement, List<int>>();
        _vertices = new List<Vector2>();
        _colors = new List<Color>();
    }

    public abstract Color GetColor(TElement poly, Data d);
    public abstract IEnumerable<Triangle> GetTris(TElement e, Data d);
    public abstract IEnumerable<TElement> GetElements(Data d);

    protected void DrawFirst(Data d)
    {
        int iter = 0;
        var elements = GetElements(d);
        foreach (var element in elements)
        {
            var tris = GetTris(element, d);
            var triIndices = new List<int>();
            _triIndicesByElement.Add(element, triIndices);
            var color = GetColor(element, d);
            
            foreach (var tri in tris)
            {
                triIndices.Add(iter);
                _colors.Add(color);
                _vertices.Add(tri.A);
                _vertices.Add(tri.B);
                _vertices.Add(tri.C);
                iter++;
            }
        }

        if (_vertices.Count < 3) _arrayMesh = new ArrayMesh();
        else _arrayMesh = MeshGenerator.GetArrayMesh(_vertices.ToArray(), _colors.ToArray());
        Mesh = _arrayMesh;
    }
    public void Draw(Data d)
    {
        if (_vertices.Count < 3) return;
        var mdt = new MeshDataTool();
        mdt.CreateFromSurface(_arrayMesh, 0);
        _arrayMesh.ClearSurfaces();
        foreach (var key in _triIndicesByElement.Keys)
        {
            var tris = _triIndicesByElement[key];
            var color = GetColor(key, d);
            foreach (var tri in tris)
            {
                mdt.SetVertexColor(tri*3, color);
                mdt.SetVertexColor(tri*3 + 1, color);
                mdt.SetVertexColor(tri*3 + 2, color);
            }
        }
    
        mdt.CommitToSurface(_arrayMesh);
    }
}
