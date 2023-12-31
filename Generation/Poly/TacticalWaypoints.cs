
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class TacticalWaypoints : Entity
{
    public Dictionary<int, Waypoint> Waypoints { get; private set; }
    public Dictionary<int, HashSet<int>> PolyAssocWaypoints { get; private set; }
    public Dictionary<Vector2, int> ByPos { get; private set; }
    public Dictionary<int, int> OccupierRegimes { get; private set; }
    public Dictionary<int, int> PolyCenterWpIds { get; private set; }
    public static TacticalWaypoints Create(GenWriteKey key,
        Dictionary<int, Waypoint> wps)
    {
        var polyAssocWaypoints = new Dictionary<int, HashSet<int>>();
        var byPos = new Dictionary<Vector2, int>();
        var occupierRegimes = new Dictionary<int, int>();
        foreach (var wp in wps.Values)
        {
            byPos.Add(wp.Pos, wp.Id);
            foreach (var poly in wp.AssocPolys(key.Data))
            {
                polyAssocWaypoints
                    .GetOrAdd(poly.Id, p => new HashSet<int>())
                    .Add(wp.Id);
            }
        }

        
        
        if (key.Data.GetAll<MapPolygon>().Any(p => polyAssocWaypoints.ContainsKey(p.Id) == false))
        {
            throw new Exception();
        }
        var n = new TacticalWaypoints(key.Data.IdDispenser.TakeId(), 
            wps,
            polyAssocWaypoints,
            byPos,
            occupierRegimes, 
            new Dictionary<int, int>());
        key.Create(n);
        return n;
    }
    [SerializationConstructor] private TacticalWaypoints(int id,
        Dictionary<int, Waypoint> waypoints,
        Dictionary<int, HashSet<int>> polyAssocWaypoints,
        Dictionary<Vector2, int> byPos,
        Dictionary<int, int> occupierRegimes,
        Dictionary<int, int> polyCenterWpIds) : base(id)
    {
        Waypoints = waypoints;
        PolyAssocWaypoints = polyAssocWaypoints;
        ByPos = byPos;
        OccupierRegimes = occupierRegimes;
        PolyCenterWpIds = polyCenterWpIds;
    }

    public void SetInitialOccupiers(GenWriteKey key)
    {
        foreach (var wp in Waypoints.Values)
        {
            if (wp is IWaterWaypoint) continue;
            foreach (var poly in wp.AssocPolys(key.Data))
            {
                if (poly.IsWater()) continue;
                if (poly.OccupierRegime.Fulfilled())
                {
                    var r = poly.OwnerRegime
                        .Entity(key.Data);
                    OccupierRegimes[wp.Id] = r.Id;
                }
            }

            if (OccupierRegimes.ContainsKey(wp.Id) == false)
            {
                OccupierRegimes[wp.Id] = -1;
            }
        }
    }
}