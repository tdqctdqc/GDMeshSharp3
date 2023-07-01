using System;
using System.Collections.Generic;
using System.Linq;

public static class MapPolyNexusExt
{
    public static bool IsRiverNexus(this MapPolyNexus nexus)
    {
        return nexus.IncidentEdges.Any(e => e.IsRiver());
    }
}
