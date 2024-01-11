using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class RiverWaypoint : Waypoint, IWaterWaypoint, IRiverWaypoint
{
    public bool HasBridge { get; private set; }
    public bool Bridgeable { get; private set; }

    public RiverWaypoint(GenWriteKey key, int id, 
        PolyTriPosition tri, Vector2 pos, 
        MapPolygon poly1, MapPolygon poly2 = null, MapPolygon poly3 = null, MapPolygon poly4 = null) 
        : base(key, id, tri, pos, poly1, poly2, poly3, poly4)
    {
        HasBridge = false;
    }

    [SerializationConstructor] private RiverWaypoint(int id, bool hasBridge, 
        bool bridgeable,
        HashSet<int> neighbors, Vector4I associatedPolyIds, Vector2 pos,
        EntityRef<Alliance> controller)
        : base(id, neighbors, associatedPolyIds, pos, controller)
    {
        Bridgeable = bridgeable;
        HasBridge = hasBridge;
    }

    public void MakeBridgeable(GenWriteKey key)
    {
        Bridgeable = true;
    }
}
