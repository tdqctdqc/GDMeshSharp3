
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

[MessagePack.Union(0, typeof(LandCell))]
[MessagePack.Union(1, typeof(RiverCell))]
[MessagePack.Union(2, typeof(SeaCell))]
public abstract class PolyCell : IPolymorph
{
    public int Id { get; private set; }
    public HashSet<int> Neighbors { get; private set; }
    public Vector2 RelTo { get; private set; }
    public Vector2[] RelBoundary { get; private set; }
    public ModelRef<Vegetation> Vegetation { get; private set; }
    public ModelRef<Landform> Landform { get; private set; }
    
    protected PolyCell(Vector2 relTo, 
        Vector2[] relBoundary,
        ModelRef<Vegetation> vegetation,
        ModelRef<Landform> landform,
        HashSet<int> neighbors,
        int id)
    {
        Id = id;
        RelTo = relTo;
        RelBoundary = relBoundary;
        Landform = landform;
        Vegetation = vegetation;
        Neighbors = neighbors;
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
            float tolerance = .1f;
            var close = p.GetClosestPointOnLineSegment(
                new Vector2(seg.X, seg.Y), new Vector2(seg.Z, seg.W));
            return close.DistanceTo(p) <= tolerance;
        }
    }

    public void SetBoundary(Vector2[] boundary, GenWriteKey key)
    {
        RelBoundary = boundary;
    }
}