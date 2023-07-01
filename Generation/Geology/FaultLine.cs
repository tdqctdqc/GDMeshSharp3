using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FaultLine 
{
    public List<List<LineSegment>> Segments { get; private set; }
    public GenPlate LowId { get; private set; }
    public GenPlate HighId { get; private set; }
    public List<MapPolygon> PolyFootprint { get; private set; }
    public float Friction { get; private set; }
    public MapPolygon Origin => HighId.GetSeedPoly();
    public FaultLine(float friction, GenPlate highId, 
        GenPlate lowId, List<PolyBorderChain> edgesHi,
        GenData data)
    {
        Friction = friction;
        HighId = highId;
        LowId = lowId;
        PolyFootprint = new List<MapPolygon>();
        Segments = edgesHi.Select(e => e.Segments).ToList();
        Segments.ForEach(ss => ss.ForEach(s => s.Clamp(data.Planet.Width)));
    }

    public LineSegment GetClosestSeg(MapPolygon poly, GenData data)
    {
        return Segments.SelectMany(s => s)
            .OrderBy(s => s.DistanceTo(Origin.GetOffsetTo(poly, data)))
            .First();
    }
    public float GetDist(MapPolygon poly, GenData data)
    {
        return GetClosestSeg(poly, data).DistanceTo(Origin.GetOffsetTo(poly, data));
    }
    public bool PointWithinDist(Vector2 pointAbs, float dist, GenData data)
    {
        return Segments.Any(seg => seg.Any(l => l.DistanceTo(Origin.GetOffsetTo(pointAbs, data)) < dist));
    }
}