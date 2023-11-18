
using System.Collections.Generic;
using System.Linq;

public static class WaypointExt
{
    public static ForceBalance GetForceBalance(this Waypoint wp, Data d)
    {
        return d.Context.WaypointForceBalances[wp];
    }
    public static IEnumerable<Waypoint> GetNeighboringNavWaypoints(this Waypoint wp, Data d)
    {
        return wp.Neighbors.Select(i => d.Planet.NavWaypoints.Waypoints[i]);
    }
    public static IEnumerable<Waypoint> GetNeighboringTacWaypoints(this Waypoint wp, Data d)
    {
        return wp.Neighbors.Select(i => d.Military.TacticalWaypoints.Waypoints[i]);
    }
}