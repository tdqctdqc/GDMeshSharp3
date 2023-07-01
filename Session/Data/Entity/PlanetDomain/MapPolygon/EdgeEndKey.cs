using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public struct EdgeEndKey
{
    public MapPolyNexus Nexus { get; private set; }
    public MapPolygonEdge Edge { get; private set; }

    public EdgeEndKey(MapPolyNexus nexus, MapPolygonEdge edge)
    {
        Nexus = nexus;
        Edge = edge;
    }
}
