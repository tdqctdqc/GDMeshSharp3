using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class InlandWaypoint : Waypoint, ILandWaypoint
{
    public float Roughness { get; private set; }

    public InlandWaypoint(GenWriteKey key, int id, 
        Vector2 pos, PolyTriPosition tri, MapPolygon poly1, 
        MapPolygon poly2 = null, 
        MapPolygon poly3 = null, 
        MapPolygon poly4 = null) 
        : base(key, id, tri, pos, poly1, poly2, poly3, poly4)
    {
    }

    [SerializationConstructor] private InlandWaypoint(int id, float roughness, 
        HashSet<int> neighbors, Vector4I associatedPolyIds, Vector2 pos, 
        EntityRef<Alliance> controller) 
        : base(id, neighbors, associatedPolyIds, pos, controller)
    {
        Roughness = roughness;
    }

    public void SetRoughness(float roughness, GenWriteKey key)
    {
        Roughness = roughness;
    }
    
}
