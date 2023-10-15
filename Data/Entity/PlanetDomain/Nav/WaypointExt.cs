
using System.Collections.Generic;
using System.Linq;

public static class WaypointExt
{
    public static IEnumerable<Waypoint> GetNeighboringWaypoints(this Waypoint wp, Data d)
    {
        return wp.Neighbors.Select(i => d.Planet.Nav.Waypoints[i]);
    }
}