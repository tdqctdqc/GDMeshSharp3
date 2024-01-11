using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class CoastWaypoint : Waypoint, ICoastWaypoint, ILandWaypoint
{
    public float Roughness { get; private set; }
    public int Sea { get; private set; }
    public bool Port { get; private set; }
    public CoastWaypoint(GenWriteKey key, int sea, 
        bool port, int id, PolyTriPosition tri,
        Vector2 pos, MapPolygon poly1, 
        MapPolygon poly2 = null, 
        MapPolygon poly3 = null, 
        MapPolygon poly4 = null) 
        : base(key, id, tri, pos, poly1, poly2, poly3, poly4)
    {
        Sea = sea;
        Port = port;
    }

    [SerializationConstructor] private CoastWaypoint(int id, 
        float roughness, HashSet<int> neighbors, Vector4I associatedPolyIds, 
        Vector2 pos, int sea, bool port, EntityRef<Alliance> controller) 
        : base(id, neighbors, associatedPolyIds, pos, controller)
    {
        Roughness = roughness;
        Sea = sea;
        Port = port;
    }

    public void SetPort(bool port, GenWriteKey key)
    {
        Port = port;
    }

    public void SetRoughness(float roughness, GenWriteKey key)
    {
        Roughness = roughness;
    }


}
