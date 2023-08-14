using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class LandNav : WaypointType
{
    public Vector2[] LfProportions { get; private set; }
    public Vector2[] VegProportions { get; private set; }
    public float Roughness { get; private set; }

    public bool HasRoadWith(Waypoint p)
    {
        if (p.WaypointType.Value() is SeaNav) return false;
        return false;
    }
}
