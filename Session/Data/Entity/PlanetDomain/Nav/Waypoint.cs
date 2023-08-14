using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class Waypoint
{
    public int Id { get; private set; }
    public HashSet<int> Neighbors { get; private set; }
    public Vector2 Pos { get; private set; }
    public Vector2 ChunkCoords { get; private set; }
    public PolymorphMember<WaypointType> WaypointType { get; private set; }

    public static Waypoint Construct(GenWriteKey key, int id, MapPolygon poly, Vector2 pos)
    {
        return new Waypoint(id, poly.GetChunk(key.Data).Coords, new HashSet<int>(), pos, 
            PolymorphMember<WaypointType>.Construct(null));
    }
    [SerializationConstructor] private Waypoint(int id, Vector2 chunkCoords, HashSet<int> neighbors, Vector2 pos,
        PolymorphMember<WaypointType> waypointType)
    {
        Id = id;
        ChunkCoords = chunkCoords;
        Neighbors = neighbors;
        Pos = pos;
        WaypointType = waypointType;
    }

    public void SetType(WaypointType t, GenWriteKey key)
    {
        WaypointType = PolymorphMember<WaypointType>.Construct(t);
    }
}
