using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class TriColorMesh<TElement> : MeshInstance2D, IMapChunkGraphicNode
{
    public string Name { get; private set; }
    public Node2D Node => this;
    private Dictionary<TElement, List<int>> _triIndicesByElement;
    public HashSet<TElement> Updates { get; private set; }
    private Func<TElement, Data, Color> _getColor;
    private Func<TElement, Data, IEnumerable<Triangle>> _getTris;
    private Func<Data, IEnumerable<TElement>> _getElements;
    private ArrayMesh _arrayMesh;
    public TriColorMesh(string name, Func<TElement, Data, Color> getColor,
        Func<TElement, Data, IEnumerable<Triangle>> getTris, Func<Data, IEnumerable<TElement>> getElements)
    {
        Name = name;
        _triIndicesByElement = new Dictionary<TElement, List<int>>();
        Updates = new HashSet<TElement>();
        _getColor = getColor;
        _getTris = getTris;
        _getElements = getElements;
    }

    public void Init(Data data)
    {
        var vertices = new List<Vector2>();
        var colors = new List<Color>();
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
                colors.Add(color);
                vertices.Add(tri.A);
                vertices.Add(tri.B);
                vertices.Add(tri.C);
                iter++;
            }
        }

        _arrayMesh = MeshGenerator.GetArrayMesh(vertices.ToArray(), colors.ToArray());
        Mesh = _arrayMesh;
    }
    public void Redraw(Data data)
    {
        Updates.AddRange(_triIndicesByElement.Keys);
        Update(data);
    }
    public void ChangeColorFunc(Func<TElement, Data, Color> newGetColor, Data data)
    {
        _getColor = newGetColor;
        Updates.AddRange(_triIndicesByElement.Keys);
        Update(data);
    }
    public void Update(Data d)
    {
        var mdt = new MeshDataTool();
        mdt.CreateFromSurface(_arrayMesh, 0);
        foreach (var key in Updates)
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
    }

    
}
