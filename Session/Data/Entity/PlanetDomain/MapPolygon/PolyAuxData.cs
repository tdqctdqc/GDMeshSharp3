using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Godot;

public class PolyAuxData
{
    public bool Stale { get; private set; }
    public Vector2 GraphicalCenter { get; private set; }
    public List<LineSegment> OrderedBoundarySegs => _orderedBoundarySegs;
    private List<LineSegment> _orderedBoundarySegs;
    public Vector2[] OrderedBoundaryPoints => _orderedBoundaryPoints;
    private Vector2[] _orderedBoundaryPoints;
    public PolyAuxData(MapPolygon p, Data data)
    {
        Update(p, data);
    }

    public void Update(MapPolygon p, Data data)
    {
        var nbs = p.NeighborBorders.Values.ToList();
        if (nbs.Count() > 0)
        {
            var source = p.Neighbors.Entities(data).Select(n => p.GetBorder(n.Id).Segments).ToList();
            MakeBoundarySegs(p, data, source);   
        }
    }

    private void MakeBoundarySegs(MapPolygon p, Data data, List<List<LineSegment>> source)
    {
        List<LineSegment> ordered;
        try
        {
            ordered = source.Chainify();
        }
        catch
        {
            var ex = new GeometryException("couldnt make boundary segs");
            ex.AddSegLayer(source.SelectMany(l => l).ToList(), "source neighbor segs");
            ex.AddSegLayer(p.Neighbors.Entities(data)
            .Select(n => p.GetOffsetTo(n, data))
            .Select(o => new LineSegment(Vector2.Zero, o))
            .ToList(), "neighbors");
            
            ex.AddSegLayer(data.GetAll<MapPolygon>()
                .Where(e => p.GetOffsetTo(e, data).Length() < 1000f)
                .Select(n => p.GetOffsetTo(n, data))
                .Select(o => new LineSegment(Vector2.Zero, o))
                .ToList(), "near");
            if(_orderedBoundarySegs != null) ex.AddSegLayer(_orderedBoundarySegs, "old segs");
            throw ex;
        }
        
        if (ordered.IsChain() == false)
        {
            var e = new GeometryException("couldnt order boundary");
            e.AddSegLayer(_orderedBoundarySegs, "old");
            e.AddSegLayer(ordered, "new");
            throw e;
        }
        ordered.CompleteCircuit();
        
        if (ordered.Any(ls => ls.From == ls.To))
        {
            var e = new GeometryException("degenerate seg");
            e.AddSegLayer(_orderedBoundarySegs, "old");
            e.AddSegLayer(ordered, "new");
            throw e;
        }

        _orderedBoundarySegs = ordered;
        _orderedBoundaryPoints = ordered.GetPoints().ToArray();
        GraphicalCenter = OrderedBoundarySegs.Average();
    }

    public bool PointInPoly(MapPolygon poly, Vector2 pointRel, Data data)
    
    {
        // return poly.GetOrderedBoundarySegs(data)
        //     .Any(s => TriangleExt.ContainsPoint(s.From, s.To, Vector2.Zero, pointRel));
        return Geometry2D.IsPointInPolygon(pointRel, _orderedBoundaryPoints);
    }
    public void MarkStale(GenWriteKey key)
    {
        Stale = true;
    }

    public void MarkFresh()
    {
        Stale = false;
    }
}
