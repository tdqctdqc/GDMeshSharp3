using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FaultLine 
{
    public List<LineSegment> Segments { get; private set; }
    public GenPlate LowId { get; private set; }
    public GenPlate HighId { get; private set; }
    public float Friction { get; private set; }
    public MapPolygon Origin => HighId.GetSeedPoly();
    public FaultLine(float friction, GenPlate highId, 
        GenPlate lowId, List<MapPolygonEdge> edges,
        GenData data)
    {
        Friction = friction;
        HighId = highId;
        LowId = lowId;
        Segments = new List<LineSegment>();



        edges.ForEach(e =>
        {
            var hi = e.HighPoly.Get(data);
            var lo = e.LowPoly.Get(data);
            var cells = data.GenAuxData.PreCellPolys[hi];
                
            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                for (var j = 0; j < cell.Neighbors.Count; j++)
                {
                    var nCell = cell.Neighbors[j];
                    if (nCell.PrePoly.Id == lo.Id)
                    {
                        var edge = cell.EdgesRel[j];
                        var from = edge.Item1 + cell.RelTo;
                        var to = edge.Item2 + cell.RelTo;
                        Segments.Add(new LineSegment(from, to));
                    }
                }
            }
        });
        Segments.ForEach(ss => ss.Clamp(data.Planet.Width));
    }

    public LineSegment GetClosestSeg(Vector2 pos, GenData data)
    {
        return Segments
            .OrderBy(s => s.DistanceTo(Origin.GetOffsetTo(pos, data)))
            .First();
    }
    public float GetDist(Vector2 pos, GenData data)
    {
        return GetClosestSeg(pos, data).DistanceTo(Origin.GetOffsetTo(pos, data));
    }
    public bool PointWithinDist(Vector2 pointAbs, float dist, GenData data)
    {
        return Segments.Any(seg => seg.DistanceTo(Origin.GetOffsetTo(pointAbs, data)) < dist);
    }
}