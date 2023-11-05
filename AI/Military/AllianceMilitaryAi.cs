
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AllianceMilitaryAi
{
    private Alliance _alliance;
    public List<List<Waypoint>> FrontlineWaypoints { get; private set; }
    public HashSet<Waypoint> FrontHash { get; private set; }
    public HashSet<Waypoint> ContactLineHash { get; private set; }
    public AllianceMilitaryAi(Alliance alliance)
    {
        _alliance = alliance;
        FrontlineWaypoints = new List<List<Waypoint>>();
        FrontHash = new HashSet<Waypoint>();
        ContactLineHash = new HashSet<Waypoint>();
    }
    public void Calculate(LogicWriteKey key, Alliance alliance, 
        AllianceMajorTurnOrders orders)
    {
        if (key.Data.Context.ControlledAreas.ContainsKey(alliance) == false)
        {
            GD.Print("no control areas for alliance at poly " 
                     + alliance.Leader.Entity(key.Data).GetPolys(key.Data).First().Id);
            return;
        }
        var controlled = 
            key.Data.Context.ControlledAreas[alliance];
        
        CalculateFrontWaypoints(controlled, orders, key.Data);
        var uncovered = FindUncoveredFrontlineWaypoints(FrontHash, orders, key.Data);
        CoverUncoveredFrontlines(uncovered, orders, key);
    }

    private void CalculateFrontWaypoints(IEnumerable<Waypoint> controlled, 
        AllianceMajorTurnOrders orders, Data d)
    {
        var forceBalances = d.Context.WaypointForceBalances;
        var frontWps = controlled.Where(front);
        FrontlineWaypoints = UnionFind.Find(frontWps,
            (wp1, wp2) => true,
            wp => wp.GetNeighboringWaypoints(d));
        FrontHash = FrontlineWaypoints.SelectMany(v => v).ToHashSet();
        bool front(Waypoint wp)
        {
            return hostileWaypoint(wp) 
                   || wp.GetNeighboringWaypoints(d)
                       .Any(n => hostileWaypoint(n));
        }
        bool hostileWaypoint(Waypoint wp)
        {
            if (forceBalances.ContainsKey(wp) == false) return false;
            return forceBalances[wp]
                .GetControllingAlliances()
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
            var fronts = data.Military.FrontAux.Fronts[regime];
            if (fronts == null) continue;
            foreach (var front in fronts)
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
        AllianceMajorTurnOrders orders, LogicWriteKey key)
    {
        var forceBalances = key.Data.Context.WaypointForceBalances;

        foreach (var uncoveredUnion in uncoveredUnions)
        {
            var regime = GetRegimeToCover(uncoveredUnion, key.Data);
            var front = Front.Create(regime, uncoveredUnion.Select(wp => wp.Id), key);
        }
    }

    private Regime GetRegimeToCover(List<Waypoint> uncovered, Data data)
    {
        return _alliance.Members.Items(data).First();
    }
}