
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
    public static IEnumerable<Waypoint> TacNeighbors(this Waypoint wp, Data d)
    {
        return wp.Neighbors.Select(i => d.Military.TacticalWaypoints.Waypoints[i]);
    }
    public static bool IsDirectlyThreatened(this Waypoint wp,
        Alliance alliance, Data data)
    {
        var controlling = data.Context
            .WaypointForceBalances[wp]
            .GetControllingAlliances();
        return controlling
            .Any(a => alliance.Rivals.Contains(a));
    }
    
    public static bool IsIndirectlyThreatened(this Waypoint wp, 
        Alliance alliance, Data data)
    {
        return wp.TacNeighbors(data)
            .Any(n => n.IsDirectlyThreatened(alliance, data));
    }
}