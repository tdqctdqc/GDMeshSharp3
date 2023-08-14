using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class RoadSegment : Entity
{
    public EntityRef<MapPolygonEdge> Edge { get; protected set; }
    public List<PolyTri> Tris { get; private set; }
    public ModelRef<RoadModel> Road { get; protected set; }
    [SerializationConstructor] private RoadSegment(int id, EntityRef<MapPolygonEdge> edge,
        List<PolyTri> tris,
        ModelRef<RoadModel> road) : base(id)
    {
        Edge = edge;
        Tris = tris;
        Road = road;
    }
    
    public static RoadSegment Create(MapPolygonEdge edge, RoadModel road, CreateWriteKey key)
    {
        var rs =  new RoadSegment(-1, edge.MakeRef(), new List<PolyTri>(), road.MakeRef());
        key.Create(rs);
        return rs;
    }
}