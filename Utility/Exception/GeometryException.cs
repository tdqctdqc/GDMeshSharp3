using System.Collections.Generic;
using System.Linq;
using Godot;

public class GeometryException : DisplayableException
{
    public string Message { get; private set; }
    public List<List<LineSegment>> SegLayers { get; private set; }
    public List<string> SegLayerNames { get; private set; }
    public List<List<Vector2>> PointSets { get; private set; }
    public List<string> PointSetNames { get; private set; }
    public List<List<ColorTri>> TriSets { get; private set; }
    public List<string> TriSetNames { get; private set; }

    public GeometryException(string message)
    {
        Message = message;
        SegLayers = new List<List<LineSegment>>();
        SegLayerNames = new List<string>();
        PointSets = new List<List<Vector2>>();
        PointSetNames = new List<string>();
        TriSets = new List<List<ColorTri>>();
        TriSetNames = new List<string>();
    }

    public void AddSegLayer(List<LineSegment> lines, string name)
    {
        SegLayers.Add(lines.ToList());
        SegLayerNames.Add(name);
    }

    public void AddPointSet(List<Vector2> pointSet, string name)
    {
        PointSets.Add(pointSet);
        PointSetNames.Add(name);
    }

    public void AddTriSet<T>(List<T> triSet, string name) where T : Triangle
    {
        TriSets.Add(triSet.Select(t => new ColorTri(ColorsExt.GetRandomColor(), t.A, t.B, t.C)).ToList());
        TriSetNames.Add(name);    
    }
    public void AddTriSet(List<ColorTri> triSet, string name)
    {
        TriSets.Add(triSet);
        TriSetNames.Add(name);
    }
    public override Node2D GetGraphic()
    {
        var d = SceneManager.Instance<GeometryExceptionDisplay>();
        d.Setup(this);
        return d;
    }

    public override Control GetUi()
    {
        throw new System.NotImplementedException();
    }
}
