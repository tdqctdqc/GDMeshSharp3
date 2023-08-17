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
    public PolymorphMember<WaypointData> WaypointData { get; private set; }

    public static Waypoint Construct(GenWriteKey key, int id, MapPolygon poly, Vector2 pos)
    {
        return new Waypoint(id, poly.GetChunk(key.Data).Coords, new HashSet<int>(), pos, 
            PolymorphMember<WaypointData>.Construct(null));
    }
    [SerializationConstructor] private Waypoint(int id, Vector2 chunkCoords, HashSet<int> neighbors, Vector2 pos,
        PolymorphMember<WaypointData> waypointData)
    {
        Id = id;
        ChunkCoords = chunkCoords;
        Neighbors = neighbors;
        Pos = pos;
        WaypointData = waypointData;
    }

    public void SetType(WaypointData t, GenWriteKey key)
    {
        WaypointData = PolymorphMember<WaypointData>.Construct(t);
    }
}
