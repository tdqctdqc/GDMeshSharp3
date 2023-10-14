using System;
using System.Collections.Generic;
using System.Linq;

public struct PolyCornerKey
{
    public MapPolygon Poly { get; private set; }
    public MapPolyNexus Nexus { get; private set; }

    public PolyCornerKey(MapPolyNexus nexus, MapPolygon poly)
    {
        Poly = poly;
        Nexus = nexus;
    }
}
