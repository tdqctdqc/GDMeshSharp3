
using System.Collections.Generic;
using Godot;
using MessagePack;

public class RiverMouthWaypoint : Waypoint, IRiverWaypoint, 
    ICoastWaypoint, IWaterWaypoint
{
    public bool HasBridge { get; private set; }
    public bool Bridgeable { get; private set; }
    public int Sea { get; private set; }
    public bool Port { get; private set; }
    
    public RiverMouthWaypoint(GenWriteKey key, int id, Vector2 pos, int sea, MapPolygon poly1, 
        MapPolygon poly2 = null, MapPolygon poly3 = null, MapPolygon poly4 = null) 
        : base(key, id, pos, poly1, poly2, poly3, poly4)
    {
        Sea = sea;
        Bridgeable = true;
        HasBridge = false;
    }
    [SerializationConstructor] private RiverMouthWaypoint(int id, int sea, bool hasBridge,
        bool bridgeable,
        bool port, HashSet<int> neighbors, Vector4I associatedPolyIds, 
        EntityRef<Alliance> controller, Vector2 pos) 
        : base(id, neighbors, associatedPolyIds, pos, controller)
    {
        Bridgeable = bridgeable;
        HasBridge = hasBridge;
        Sea = sea;
        Port = port;
    }
    public void SetPort(bool port, GenWriteKey key)
    {
        Port = port;
    }
    
    public override float GetDefendCost(Data data)
    {
        return 1f;
    }
}