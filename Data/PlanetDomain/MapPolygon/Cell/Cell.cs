
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

[MessagePack.Union(0, typeof(LandCell))]
[MessagePack.Union(1, typeof(RiverCell))]
[MessagePack.Union(2, typeof(SeaCell))]
public abstract class Cell : IPolymorph,
    IIdentifiable, ICombatGraphNode
{
    public int Id { get; private set; }
    public ERef<Regime> Controller { get; private set; }
    
    [MessagePack.IgnoreMember] public List<int> Neighbors => Geometry.Neighbors;
    [MessagePack.IgnoreMember] public List<(Vector2 f, Vector2 t)> Edges => Geometry.EdgesRel;
    [MessagePack.IgnoreMember] public Vector2 RelTo => Geometry.RelTo;
    [MessagePack.IgnoreMember] public Vector2[] RelBoundary => Geometry.PointsRel;
    
    public Vegetation GetVegetation(Data d) => Vegetation.Get(d);
    public ModelRef<Vegetation> Vegetation { get; private set; }
    public Landform GetLandform(Data d) => Landform.Get(d);
    public ModelRef<Landform> Landform { get; private set; }
    public CellGeometry Geometry { get; private set; }
        
    protected Cell(
        ModelRef<Vegetation> vegetation,
        ModelRef<Landform> landform,
        ERef<Regime> controller,
        CellGeometry geometry,
        int id)
    {
        Geometry = geometry;
        Id = id;
        Landform = landform;
        Vegetation = vegetation;
        Controller = controller;
    }

    public void SetVegetation(Vegetation v, GenWriteKey key)
    {
        Vegetation = v.MakeRef();
    }
    public void SetLandform(Landform lf, GenWriteKey key)
    {
        Landform = lf.MakeRef();
    }
    public bool AnyNeighbor(Func<Cell, bool> pred, Data d)
    {
        return Neighbors
            .Select(n => PlanetDomainExt.GetPolyCell(n, d))
            .Any(pred);
    }

    public void ForEachNeighbor(Action<Cell> act, Data d)
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

    public Vector2 GetCenter()
    {
        return RelBoundary.Avg() + RelTo;
    }

    public IEnumerable<Cell> GetNeighbors(Data d)
    {
        return Neighbors.Select(i => PlanetDomainExt.GetPolyCell(i, d));
    }

    public void SetController(Regime controller, StrongWriteKey key)
    {
        var old = Controller.Get(key.Data);
        Controller = controller.MakeRef();
        key.Data.Notices.Political.ChangedControllerRegime.Invoke(this, controller, old);
    }
    public void SetController(Regime controller, GenWriteKey key)
    {
        var old = Controller.Get(key.Data);
        Controller = controller.MakeRef();
        key.Data.Notices.Political.ChangedControllerRegime.Invoke(this, controller, old);
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

    public (Vector2, Vector2) GetEdgeRelWith(Cell n)
    {
        var index = Neighbors.IndexOf(n.Id);
        if (index == -1) throw new Exception();
        return Edges[index];
    }

    public CellRef MakeRef()
    {
        return new CellRef(Id);
    }
}