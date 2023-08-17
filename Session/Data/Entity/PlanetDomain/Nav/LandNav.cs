using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class LandNav : WaypointData
{
    public Vector2[] LfProportions { get; private set; }
    public Vector2[] VegProportions { get; private set; }
    public float Roughness { get; private set; }

    public bool HasRoadWith(Waypoint p)
    {
        if (p.WaypointData.Value() is SeaNav) return false;
        return false;
    }
}
