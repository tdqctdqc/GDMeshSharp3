using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class TriColorMesh<TElement> : MeshInstance2D, IMapChunkGraphicNode
{
    public string Name { get; private set; }
    public Node2D Node => this;
    private Dictionary<TElement, List<int>> _triIndicesByElement;
    private List<Vector2> _vertices;
    private List<Color> _colors;
    public HashSet<TElement> Updates { get; private set; }
    private Func<TElement, Data, Color> _getColor;
    private Func<TElement, Data, IEnumerable<Triangle>> _getTris;
    private Func<Data, IEnumerable<TElement>> _getElements;
    private ArrayMesh _arrayMesh;
    public TriColorMesh(string name, Func<TElement, Data, Color> getColor,
        Func<TElement, Data, IEnumerable<Triangle>> getTris, 
        Func<Data, IEnumerable<TElement>> getElements,
        Data data)
    {
        Name = name;
        Updates = new HashSet<TElement>();
        _getColor = getColor;
        _getTris = getTris;
        _getElements = getElements;
        
        
        _triIndicesByElement = new Dictionary<TElement, List<int>>();
        _vertices = new List<Vector2>();
        _colors = new List<Color>();
        int iter = 0;
        var elements = _getElements(data);
        foreach (var element in elements)
        {
            var tris = _getTris(element, data);
            var triIndices = new List<int>();
            _triIndicesByElement.Add(element, triIndices);
            var color = _getColor(element, data);
            
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
    public void Update(Data d, ConcurrentQueue<Action> queue)
    {
        if (Updates.Count == 0) return;
        // Init(d);
        queue.Enqueue(() =>
        {
            var mdt = new MeshDataTool();
            mdt.CreateFromSurface(_arrayMesh, 0);
        
            foreach (var key in _triIndicesByElement.Keys)
            {
                var tris = _triIndicesByElement[key];
                var color = _getColor(key, d);
                foreach (var tri in tris)
                {
                    mdt.SetVertexColor(tri*3, color);
                    mdt.SetVertexColor(tri*3 + 1, color);
                    mdt.SetVertexColor(tri*3 + 2, color);
                }
            }
        
            mdt.CommitToSurface(_arrayMesh);
            Updates.Clear();
        });
    }
}
