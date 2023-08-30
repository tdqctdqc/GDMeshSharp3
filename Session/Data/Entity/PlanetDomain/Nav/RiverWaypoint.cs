using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class RiverWaypoint : Waypoint, IWaterWaypoint, IRiverWaypoint
{
    public bool HasBridge { get; private set; }

    public RiverWaypoint(GenWriteKey key, int id, Vector2 pos, MapPolygon poly1, MapPolygon poly2 = null, MapPolygon poly3 = null, MapPolygon poly4 = null) : base(key, id, pos, poly1, poly2, poly3, poly4)
    {
        HasBridge = false;
    }

    [SerializationConstructor] protected RiverWaypoint(int id, bool hasBridge, Vector2 chunkCoords, 
        HashSet<int> neighbors, Vector4I associatedPolyIds, Vector2 pos)
        : base(id, chunkCoords, neighbors, associatedPolyIds, pos)
    {
        HasBridge = hasBridge;
    }
}
