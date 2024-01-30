
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
    public HashSet<int> Neighbors { get; private set; }
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
        HashSet<int> neighbors,
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
        if (RelBoundary.Length < 3) throw new Exception();
    }

    public static void ConnectCellsSharingPoints(
        IEnumerable<PolyCell> cells, Data d)
    {
        var dic = new Dictionary<Vector2, LinkedList<PolyCell>>();
        foreach (var cell in cells)
        {
            foreach (var p in cell.RelBoundary)
            {
                var i = (Vector2I)p;
                dic.GetOrAdd(i, i => new LinkedList<PolyCell>())
                    .AddLast(cell);
            }
        }
        foreach (var kvp in dic)
        {
            var pointCells = kvp.Value;
            foreach (var c1 in pointCells)
            {
                foreach (var c2 in pointCells)
                {
                    if (c1.Id == c2.Id) continue;
                    c1.Neighbors.Add(c2.Id);
                    c2.Neighbors.Add(c1.Id);
                }
            }
        }
    }

    public static void ConnectOverMapPolyEdge(
        MapPolygonEdge edge,
        Dictionary<MapPolygon, PolyCell[]> byPoly,
        Action<PolyCell, PolyCell> link,
        Data d)
    {
        // if (edge.IsRiver()) throw new Exception();
        var p1 = edge.HighPoly.Entity(d);
        var p2 = edge.LowPoly.Entity(d);

        var borderSegs1 
            = getBorderPoints(p1);
        foreach (var cell in byPoly[p2])
        {
            for (var i = 0; i < cell.RelBoundary.Length; i++)
            {
                var from = p1.Center.GetOffsetTo(cell.RelBoundary[i] + cell.RelTo, d);
                var to = p1.Center.GetOffsetTo(cell.RelBoundary.Modulo(i + 1) + cell.RelTo, d);
                for (var j = 0; j < borderSegs1.Count; j++)
                {
                    var seg = borderSegs1[j];
                    var close = Geometry2D
                        .GetClosestPointsBetweenSegments(
                            from, to, seg.Item1, seg.Item2);
                    if (close[0].DistanceTo(close[1]) < .1f)
                    {
                        link(cell, seg.Item3);
                    }
                }
            }
        }

        
        


        List<(Vector2, Vector2, PolyCell)> getBorderPoints(MapPolygon poly)
        {
            var segs = edge.GetSegsRel(poly, d).Segments;
            var borderPoints = new List<(Vector2, Vector2, PolyCell)>();
            for (var i = 0; i < byPoly[poly].Length; i++)
            {
                var cell = byPoly[poly][i];
                for (var j = 0; j < cell.RelBoundary.Length; j++)
                {
                    var from = poly.Center.GetOffsetTo(cell.RelBoundary[j] + cell.RelTo, d);
                    var to = poly.Center.GetOffsetTo(cell.RelBoundary.Modulo(j + 1) + cell.RelTo, d);
                    if (segs.Any(s =>
                        {
                            var close = Geometry2D.GetClosestPointsBetweenSegments(
                                from, to, s.From, s.To);
                            return close[0].DistanceTo(close[1]) <= .1f;
                        }))
                    {
                        borderPoints.Add((from, to, cell));
                    }
                }
            }
            return borderPoints;
        }
    }
    
    public static void ConnectCellsByEdge(IEnumerable<PolyCell> c1s,
        IEnumerable<PolyCell> edgeCells, 
        Vector2 edgeRel,
        Action<PolyCell, PolyCell> link,
        Data d)
    {
        var dic = new Dictionary<Vector4, List<PolyCell>>();
        foreach (var c2 in edgeCells)
        {
            for (var i = 0; i < c2.RelBoundary.Length; i++)
            {
                var from = c2.RelBoundary[i];
                var to = c2.RelBoundary[(i + 1) % c2.RelBoundary.Length];
                var (o1, o2) = from.Order(to);
                var key = new Vector4(o1.X, o1.Y, o2.X, o2.Y);
                dic.GetOrAdd(key, k => new List<PolyCell>())
                    .Add(c2);
            }
        }
        foreach (var c1 in c1s)
        {
            var offset = edgeRel.GetOffsetTo(c1.RelTo, d);
            foreach (var kvp in dic)
            {
                var lineSeg = kvp.Key;
                if (c1.RelBoundary.Any(p => onSegment(p + offset, lineSeg)))
                {
                    foreach (var c2 in kvp.Value)
                    {
                        link(c1, c2);
                    }
                }
            }
        }
        bool onSegment(Vector2 p, Vector4 seg)
        {
            float tolerance = 1f;
            var close = p.GetClosestPointOnLineSegment(
                new Vector2(seg.X, seg.Y), new Vector2(seg.Z, seg.W));
            return close.DistanceTo(p) <= tolerance;
        }
    }

    public void SetBoundary(Vector2[] boundary, GenWriteKey key)
    {
        RelBoundary = boundary;
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
        return Neighbors.Select(n => PlanetDomainExt.GetPolyCell(n, d))
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
        var offset = RelTo.GetOffsetTo(p, d);
        return Geometry2D.IsPointInPolygon(offset, RelBoundary);
    }
    public Vector2[] CoordinateBoundary(PolyCell coord, Data d)
    {
        return RelBoundary
            .Select(v =>
            {
                var vRel = coord.RelTo.GetOffsetTo(v + RelTo, d);
                for (var i = 0; i < coord.RelBoundary.Length; i++)
                {
                    var firstP = coord.RelBoundary[i];
                    if (firstP.DistanceTo(vRel) <= .1f)
                    {
                        vRel = firstP;
                        break;
                    }
                }

                return vRel;
            }).ToArray();
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
                relTo.GetOffsetTo(RelBoundary[tris[i]] + RelTo, d) ,
                relTo.GetOffsetTo(RelBoundary[tris[i + 1]] + RelTo, d) ,
                relTo.GetOffsetTo(RelBoundary[tris[i + 2]] + RelTo, d)));
        }
        
        return res;
    }
}