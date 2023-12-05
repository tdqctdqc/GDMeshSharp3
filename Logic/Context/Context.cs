
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Godot;

public class Context
{
    public Dictionary<Unit, Waypoint> UnitWaypoints { get; private set; }
    public Dictionary<Waypoint, ForceBalance> WaypointForceBalances { get; private set; }
    public Dictionary<Alliance, HashSet<Waypoint>> ControlledAreas { get; private set; }
    public Dictionary<Vector2, List<Waypoint>> WaypointPaths { get; private set; }
    
    public Context(Data data)
    {
        UnitWaypoints = new Dictionary<Unit, Waypoint>();
        data.Notices.MadeWaypoints.Subscribe(() => AddForceBalances(data));
        WaypointForceBalances = new Dictionary<Waypoint, ForceBalance>();
        ControlledAreas = new Dictionary<Alliance, HashSet<Waypoint>>();
        WaypointPaths = new Dictionary<Vector2, List<Waypoint>>();
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
        WaypointPaths.Clear();
        foreach (var kvp in WaypointForceBalances)
        {
            kvp.Value.ClearBalance();
        }
        CalculateUnitWaypointsAndForceBalances(data);
        CalculateOccupierRegimes(data);
        CalculateRiverControl(data);
        CalculateControlAreas(data);
    }

    
    private void CalculateUnitWaypointsAndForceBalances(Data data)
    {
        var units = data.GetAll<Unit>();
        var wpGrid = data.Military.WaypointGrid;
        
        foreach (var u in units)
        {
            if (u.Position.HasNaN()) throw new Exception();
            var found = wpGrid.TryGetClosest(u.Position, out var closeWp, 
                wp => wp is ILandWaypoint);
            if (found = false)
            {
                throw new Exception("couldnt find waypoint near " + u.Position);
            }
            UnitWaypoints[u] = closeWp;
            var forceBalance = WaypointForceBalances[closeWp];
            forceBalance.Add(u, data);
        }
    }

    private void CalculateRiverControl(Data data)
    {
        var tacWps = data.Military
            .TacticalWaypoints.Waypoints.Values;
        foreach (var wp in tacWps)
        {
            var forceBalance = wp.GetForceBalance(data);
            if (wp is IRiverWaypoint r == false) continue;
            var nRegimes = wp.TacNeighbors(data)
                .SelectMany(n => n.GetForceBalance(data).GetControllingRegimes())
                .Distinct();
            foreach (var nRegime in nRegimes)
            {
                forceBalance.Add(nRegime, 1f, data);
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
                var r = forceBalance.ByRegime
                    .Where(kvp => alliance.Members.Contains(kvp.Key))
                    .OrderBy(kvp => kvp.Value).First().Key;
                
                tacWps.OccupierRegimes[wp.Id] = r.Id;
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
    public List<Waypoint> GetWaypointPath(Waypoint start, Waypoint end, Data data)
    {
        var key = new Vector2(start.Id, end.Id);
        if (WaypointPaths.ContainsKey(key)) return WaypointPaths[key];
        
        var reverse = new Vector2(end.Id, start.Id);
        if (WaypointPaths.ContainsKey(reverse))
        {
            var reverseList = WaypointPaths[reverse].Select(w => w).Reverse().ToList();
            WaypointPaths[key] = reverseList;
            return reverseList;
        }

        var path = PathFinder.FindWaypointPath(start, end, data);
        WaypointPaths[key] = path;
        return path;
    }
}