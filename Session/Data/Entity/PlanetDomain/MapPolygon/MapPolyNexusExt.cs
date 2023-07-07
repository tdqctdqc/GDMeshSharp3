using System;
using System.Collections.Generic;
using System.Linq;

public static class MapPolyNexusExt
{
    public static bool IsRiverNexus(this MapPolyNexus nexus, Data data)
    {
        return nexus.IncidentEdges.Entities(data).Any(e => e.IsRiver());
    }
}
