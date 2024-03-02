
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

[MessagePack.Union(0, typeof(LandCell))]
[MessagePack.Union(1, typeof(RiverCell))]
[MessagePack.Union(2, typeof(SeaCell))]
public abstract class PolyCell : IPolymorph,
    IIdentifiable, ICombatGraphNode
{
    public int Id { get; private set; }
    public ERef<Regime> Controller { get; private set; }
    public List<int> Neighbors { get; private set; }
    public List<(Vector2 f, Vector2 t)> Edges { get; private set; }
    public Vector2 RelTo { get; private set; }
    public Vector2[] RelBoundary { get; private set; }
    public Vegetation GetVegetation(Data d) => Vegetation.Model(d);
    public ModelRef<Vegetation> Vegetation { get; private set; }
    public Landform GetLandform(Data d) => Landform.Model(d);
    public ModelRef<Landform> Landform { get; private set; }
    
    protected PolyCell(Vector2 relTo, 
        Vector2[] relBoundary,
        ModelRef<Vegetation> vegetation,
        ModelRef<Landform> landform,
        List<int> neighbors,
        List<(Vector2, Vector2)> edges,
        ERef<Regime> controller,
        int id)
    {
        Id = id;
        RelTo = relTo;
        RelBoundary = relBoundary;
        Landform = landform;
        Vegetation = vegetation;
        Neighbors = neighbors;
        Controller = controller;
        Edges = edges;
        if (RelBoundary.Length < 3) throw new Exception();
    }

    public void SetVegetation(Vegetation v, GenWriteKey key)
    {
        Vegetation = v.MakeRef();
    }
    public void SetLandform(Landform lf, GenWriteKey key)
    {
        Landform = lf.MakeRef();
    }

    public bool AnyNeighbor(Func<PolyCell, bool> pred, Data d)
    {
        return Neighbors
            .Select(n => PlanetDomainExt.GetPolyCell(n, d))
            .Any(pred);
    }

    public void ForEachNeighbor(Action<PolyCell> act, Data d)
    {
        foreach (var nCell in Neighbors.Select(n => PlanetDomainExt.GetPolyCell(n, d)))
        {
            act(nCell);
        }
    }
    public float Area()
    {
        var tris = Geometry2D.TriangulatePolygon(RelBoundary);
        var area = 0f;
        for (var i = 0; i < tris.Length; i+=3)
        {
            var a = RelBoundary[tris[i]];
            var b = RelBoundary[tris[i+1]];
            var c = RelBoundary[tris[i+2]];
            area += TriangleExt.GetApproxArea(a, b, c);
        }

        return area;
    }

    public bool ContainsPoint(Vector2 p, Data d)
    {
        var offset = RelTo.Offset(p, d);
        return Geometry2D.IsPointInPolygon(offset, RelBoundary);
    }

    public Vector2 GetCenter()
    {
        return RelBoundary.Avg() + RelTo;
    }

    public IEnumerable<PolyCell> GetNeighbors(Data d)
    {
        return Neighbors.Select(i => PlanetDomainExt.GetPolyCell(i, d));
    }

    public void SetController(Regime controller, StrongWriteKey key)
    {
        Controller = controller.MakeRef();
    }
    public void SetController(Regime controller, GenWriteKey key)
    {
        Controller = controller.MakeRef();
    }

    public List<Triangle> GetTriangles(Vector2 relTo, Data d)
    {
        var tris = Geometry2D.TriangulatePolygon(RelBoundary);
        var res = new List<Triangle>();
        for (var i = 0; i < tris.Length; i+=3)
        {
            res.Add(new Triangle(
                relTo.Offset(RelBoundary[tris[i]] + RelTo, d) ,
                relTo.Offset(RelBoundary[tris[i + 1]] + RelTo, d) ,
                relTo.Offset(RelBoundary[tris[i + 2]] + RelTo, d)));
        }
        
        return res;
    }

    public (Vector2, Vector2) GetEdgeRelWith(PolyCell n)
    {
        var index = Neighbors.IndexOf(n.Id);
        if (index == -1) throw new Exception();
        return Edges[index];
    }

    public void SetBoundaryPoints(Vector2[] boundary, GenWriteKey key)
    {
        RelBoundary = boundary;
    }
    protected static Vector2[] GetBoundaryPoints(List<(Vector2I, Vector2I)> edges)
    {
        var start = (Vector2)edges[0].Item1;
        return edges.Select(e => (Vector2)e.Item1)
            .Union(edges.Select(e => (Vector2)e.Item2))
            .Distinct()
            .OrderBy(p => start.AngleTo(p))
            .ToArray();
    }
}