
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Godot;

public class Context
{
    public ConcurrentDictionary<int, MovementRecord> MovementRecords { get; private set; }
    public Dictionary<Unit, Waypoint> UnitWaypoints { get; private set; }
    public Dictionary<Waypoint, ForceBalance> WaypointForceBalances { get; private set; }
    public Dictionary<Alliance, HashSet<Waypoint>> ControlledAreas { get; private set; }
    public ConcurrentDictionary<Vector2, PolyTri> PolyTris { get; private set; }
    
    public Context(Data data)
    {
        UnitWaypoints = new Dictionary<Unit, Waypoint>();
        data.Notices.MadeWaypoints.Subscribe(() => AddForceBalances(data));
        WaypointForceBalances = new Dictionary<Waypoint, ForceBalance>();
        ControlledAreas = new Dictionary<Alliance, HashSet<Waypoint>>();
        PolyTris = new ConcurrentDictionary<Vector2, PolyTri>();
        MovementRecords = new ConcurrentDictionary<int, MovementRecord>();
    }

    private void AddForceBalances(Data d)
    {
        WaypointForceBalances.Clear();
        foreach (var wp in d.Military.TacticalWaypoints.Waypoints.Values)
        {
            WaypointForceBalances.Add(wp, new ForceBalance());
        }
    }
    public void Calculate(Data data)
    {
        foreach (var kvp in WaypointForceBalances)
        {
            kvp.Value.ClearBalance();
        }
        CalculateUnitWaypointsAndForceBalances(data);
        CalculateOccupierRegimes(data);
        CalculateControlAreas(data);
    }

    public void AddToMovementRecord(int id, 
        Vector2 pos, MovementContext ctx, Data d)
    {
        var record = MovementRecords.GetOrAdd(id, i => new MovementRecord());
        record.Add((d.BaseDomain.GameClock.Tick, pos, ctx));
    }
    private void CalculateUnitWaypointsAndForceBalances(Data data)
    {
        var units = data.GetAll<Unit>();
        var wpGrid = data.Military.WaypointGrid;
        
        foreach (var u in units)
        {
            if (u.Position.Pos.HasNaN()) throw new Exception();
            var found = wpGrid.TryGetClosest(u.Position.Pos, 
                out var closeWp, 
                wp => wp is ILandWaypoint);
            if (found = false)
            {
                throw new Exception("couldnt find waypoint near " + u.Position);
            }
            UnitWaypoints[u] = closeWp;
            var occupierAlliance = closeWp.GetOccupyingRegime(data).GetAlliance(data);
            var unitAlliance = u.Regime.Entity(data).GetAlliance(data);
            if (occupierAlliance == unitAlliance || unitAlliance.AtWar.Contains(occupierAlliance))
            {
                var forceBalance = WaypointForceBalances[closeWp];
                forceBalance.Add(u, data);
            }
        }
    }
    private void CalculateOccupierRegimes(Data data)
    {
        var tacWps = data.Military
            .TacticalWaypoints;
        foreach (var wp in tacWps.Waypoints.Values)
        {
            var forceBalance = WaypointForceBalances[wp];
            var origOccupier = wp.GetOccupyingRegime(data);
            var alliance = forceBalance.GetMostPowerfulAlliance();
            if (alliance != null 
                && alliance.Members.Contains(origOccupier) == false)
            {
                var origAlliance = origOccupier.GetAlliance(data);
                var defendStr = forceBalance.ByAlliance.TryGetValue(origAlliance, out var s)
                    ? s
                    : 0f;
                var attackStr = forceBalance.ByAlliance[alliance];
                if (attackStr > 2f * defendStr)
                {
                    var r = forceBalance.ByRegime
                        .Where(kvp => alliance.Members.Contains(kvp.Key))
                        .OrderBy(kvp => kvp.Value).First().Key;
                
                    tacWps.OccupierRegimes[wp.Id] = r.Id;
                }
            }
            var occupier = wp.GetOccupyingRegime(data);
            if (occupier != null)
            {
                forceBalance.Add(occupier, 1f, data);
            }
        }
    }
    private void CalculateControlAreas(Data data)
    {
        ControlledAreas.Clear();
        foreach (var wp in data.Military.TacticalWaypoints.Waypoints.Values)
        {
            var forceBalance = WaypointForceBalances[wp];
            
            var controlling = forceBalance
                .GetControllingAlliances();
            foreach (var alliance in controlling)
            {
                ControlledAreas
                    .GetOrAdd(alliance, a => new HashSet<Waypoint>())
                    .Add(wp);
            }
        }
    }

    public PolyTri GetPolyTri(Vector2 pos, Data d)
    {
        if (pos.Y == 0)
        {
            pos += new Vector2(0f, .01f);
        }
        if (pos.Y == d.Planet.Height)
        {
            pos -= new Vector2(0f, .01f);
        }
        if (PolyTris.ContainsKey(pos)) return PolyTris[pos];
        var polyGrid = d.Planet.PolygonAux.MapPolyGrid;
        var poly = polyGrid.GetElementAtPoint(pos, d);
        var rel = poly.Center.GetOffsetTo(pos, d);
        var pt = poly.Tris.GetAtPoint(rel, d);
        PolyTris.TryAdd(pos, pt);
        return pt;
    }
}