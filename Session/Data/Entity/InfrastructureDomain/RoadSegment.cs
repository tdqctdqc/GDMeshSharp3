using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class RoadSegment : Entity
{
    public EntityRef<MapPolygonEdge> Edge { get; protected set; }
    public List<int> WaypointIds { get; private set; }
    public ModelRef<RoadModel> Road { get; protected set; }
    [SerializationConstructor] private RoadSegment(int id, EntityRef<MapPolygonEdge> edge,
        ModelRef<RoadModel> road, List<int> waypointIds) : base(id)
    {
        Edge = edge;
        Road = road;
        WaypointIds = waypointIds;
    }
    
    public static RoadSegment Create(MapPolygonEdge edge, RoadModel road, CreateWriteKey key)
    {
        var wpPath = key.Data.Planet.Nav.GetPolyPath(edge.HighPoly.Entity(key.Data),
            edge.LowPoly.Entity(key.Data)).Select(wp => wp.Id);
        var rs =  new RoadSegment(-1, edge.MakeRef(), road.MakeRef(), wpPath.ToList());
        key.Create(rs);
        return rs;
    }
}