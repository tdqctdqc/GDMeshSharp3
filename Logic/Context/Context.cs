
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
    public Context()
    {
        UnitWaypoints = new Dictionary<Unit, Waypoint>();
        WaypointForceBalances = new Dictionary<Waypoint, ForceBalance>();
        ControlledAreas = new Dictionary<Alliance, HashSet<Waypoint>>();
    }
    public void Calculate(Data data)
    {
        CalculateWaypointsAndForceBalances(data);
    }
    public void CalculateWaypointsAndForceBalances(Data data)
    {
        var sw = new Stopwatch();
        sw.Start();
        foreach (var kvp in WaypointForceBalances)
        {
            kvp.Value.Clear();
        }
        var units = data.GetAll<Unit>();
        var wpGrid = data.Planet.NavAux.WaypointGrid;
        
        foreach (var u in units)
        {
            var wp = wpGrid.GetElementAtPoint(u.Position);
            UnitWaypoints[u] = wp;
            var forceBalance = WaypointForceBalances.GetOrAdd(wp, wp => new ForceBalance());
            forceBalance.Add(u, data);
            foreach (var nWp in wp.GetNeighboringWaypoints(data))
            {
                var nForceBalance = WaypointForceBalances.GetOrAdd(nWp, wp => new ForceBalance());
                nForceBalance.DiffuseInto(u, data);
            }
        }
        
        foreach (var wp in data.Planet.Nav.Waypoints.Values)
        {
            var alliances = wp.AssocPolys(data)
                .SelectWhere(p => p.Regime.Fulfilled())
                .Select(p => p.Regime.Entity(data))
                .Select(r => r.GetAlliance(data))
                .Distinct();
            if (alliances.Count() == 0) continue;
            var forceBalance = WaypointForceBalances.GetOrAdd(wp, wp => new ForceBalance());

            foreach (var alliance in alliances)
            {
                forceBalance.Add(alliance, ForceBalance.PowerPointsForPolyOwnership);
            }

            var controlling = forceBalance.GetControllingAlliances();
            foreach (var alliance in alliances)
            {
                if (controlling.Contains(alliance))
                {
                    ControlledAreas
                        .GetOrAdd(alliance, a => new HashSet<Waypoint>())
                        .Add(wp);
                }
            }
        }
        sw.Stop();
        Game.I.Logger.Log("units " + units.Count() 
                                   + " waypoint force balance calc time " 
                                   + sw.Elapsed.TotalMilliseconds, LogType.Logic);
    }

}