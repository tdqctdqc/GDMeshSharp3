
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AllianceMilitaryAi
{
    private Alliance _alliance;
    public List<List<Waypoint>> FrontlineWaypoints { get; private set; }
    public HashSet<Waypoint> FrontlineHash { get; private set; }

    public AllianceMilitaryAi(Alliance alliance)
    {
        _alliance = alliance;
        FrontlineWaypoints = new List<List<Waypoint>>();
        FrontlineHash = new HashSet<Waypoint>();
    }
    public void Calculate(Data data, Alliance alliance, 
        AllianceMajorTurnOrders orders)
    {
        if (data.Context.ControlledAreas.ContainsKey(alliance) == false)
        {
            GD.Print("no control areas for alliance at poly " 
                     + alliance.Leader.Entity(data).GetPolys(data).First().Id);
            return;
        }
        var controlled = 
            data.Context.ControlledAreas[alliance];
        
        CalculateFrontlineWaypoints(controlled, orders, data);
        var uncovered = FindUncoveredFrontlineWaypoints(FrontlineHash, orders, data);
        CoverUncoveredFrontlines(uncovered, orders, data);
    }

    private void CalculateFrontlineWaypoints(IEnumerable<Waypoint> controlled, 
        AllianceMajorTurnOrders orders, Data d)
    {
        var forceBalances = d.Context.WaypointForceBalances;
        var frontlineWps = controlled.Where(frontline);
        FrontlineWaypoints = UnionFind.Find(frontlineWps,
            (wp1, wp2) => true,
            wp => wp.GetNeighboringWaypoints(d));
        FrontlineHash = FrontlineWaypoints.SelectMany(v => v).ToHashSet();
        
        bool frontline(Waypoint wp)
        {
            return wp.GetNeighboringWaypoints(d)
                .Any(n => hostileWaypoint(n));
        }
        bool hostileWaypoint(Waypoint wp)
        {
            if (forceBalances.ContainsKey(wp) == false) return false;
            return forceBalances[wp].GetControllingAlliances()
                .Any(a => hostileAlliance(a));
        }
        bool hostileAlliance(Alliance a)
        {
            if (a == null) return false;
            return _alliance.Rivals.Contains(a) || _alliance.AtWar.Contains(a);
        }
    }

    private List<List<Waypoint>> FindUncoveredFrontlineWaypoints(HashSet<Waypoint> frontlineHash, 
        AllianceMajorTurnOrders orders,
        Data data)
    {
        var uncovered = frontlineHash.ToHashSet();
        foreach (var regime in _alliance.Members.Items(data))
        {
            foreach (var front in regime.Military.Fronts.Items(data))
            {
                foreach (var wp in front.WaypointIds)
                {
                    uncovered.Remove(data.Planet.Nav.Waypoints[wp]);
                }
            }
        }
        
        return UnionFind.Find(
            uncovered, (w, v) => true,
            w => w.GetNeighboringWaypoints(data));
    }

    private void CoverUncoveredFrontlines(List<List<Waypoint>> uncoveredUnions,
        AllianceMajorTurnOrders orders, Data data)
    {
        var forceBalances = data.Context.WaypointForceBalances;

        foreach (var uncoveredUnion in uncoveredUnions)
        {
            var regime = GetRegimeToCover(uncoveredUnion, data);
            orders.NewFrontWaypointsByRegimeId.Add((regime.Id,
                uncoveredUnion.Select(wp => wp.Id).ToList()));
        }
    }

    private Regime GetRegimeToCover(List<Waypoint> uncovered, Data data)
    {
        return _alliance.Members.Items(data).First();
    }
}