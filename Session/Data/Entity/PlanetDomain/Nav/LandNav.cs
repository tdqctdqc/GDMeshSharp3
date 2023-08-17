using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public abstract class LandNav : WaypointData
{
    public float Roughness { get; private set; }

    [SerializationConstructor] protected LandNav(float roughness)
    {
        Roughness = roughness;
    }
    public bool HasRoadWith(Waypoint p)
    {
        if (p.WaypointData.Value() is SeaNav) return false;
        return false;
    }

    public void SetRoughness(float roughness, GenWriteKey key)
    {
        Roughness = roughness;
    }
}
