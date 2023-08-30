using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class InlandWaypoint : Waypoint, ILandWaypoint
{
    public float Roughness { get; private set; }

    public InlandWaypoint(GenWriteKey key, int id, Vector2 pos, MapPolygon poly1, 
        MapPolygon poly2 = null, MapPolygon poly3 = null, MapPolygon poly4 = null) 
        : base(key, id, pos, poly1, poly2, poly3, poly4)
    {
    }

    [SerializationConstructor] protected InlandWaypoint(int id, float roughness, Vector2 chunkCoords, 
        HashSet<int> neighbors, Vector4I associatedPolyIds, Vector2 pos) 
        : base(id, chunkCoords, neighbors, associatedPolyIds, pos)
    {
        Roughness = roughness;
    }

    public void SetRoughness(float roughness, GenWriteKey key)
    {
        Roughness = roughness;
    }
}
