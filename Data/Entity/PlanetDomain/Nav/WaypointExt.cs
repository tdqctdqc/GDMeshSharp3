using System.Collections.Generic;
using System.Linq;

public static class WaypointExt
{
    public static RoadModel GetRoadWith(this Waypoint w, Waypoint v, Data d)
    {
        return d.Infrastructure.RoadNetwork.Roads[w, v]?.Model(d);
    }
    public static Regime GetOccupyingRegime(this Waypoint wp, Data d)
    {
        if (d.Military.TacticalWaypoints.OccupierRegimes.ContainsKey(wp.Id) == false) return null;
        var id = d.Military.TacticalWaypoints.OccupierRegimes[wp.Id];
        if (id == -1) return null;
        return d.Get<Regime>(id);
    }
    public static ForceBalance GetForceBalance(this Waypoint wp, Data d)
    {
        return d.Context.WaypointForceBalances[wp];
    }
    public static IEnumerable<Waypoint> TacNeighbors(this Waypoint wp, Data d)
    {
        return wp.Neighbors.Select(i => MilitaryDomain.GetTacWaypoint(i, d));
    }

    public static bool IsControlled(this Waypoint wp,
        Alliance alliance, Data data)
    {
        return data.Context
            .WaypointForceBalances[wp].IsAllianceControlling(alliance);
    }

    public static bool IsThreatened(this Waypoint wp,
        Alliance alliance, Data data)
    {
        return wp.IsDirectlyThreatened(alliance, data)
               || wp.IsIndirectlyThreatened(alliance, data);
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
            .Any(n =>
                n.IsDirectlyThreatened(alliance, data) && n.IsControlled(alliance, data) == false
            );
    }

    public static bool Neighbors(this Waypoint w, Waypoint v)
    {
        return w.Neighbors.Contains(v.Id);
    }
}