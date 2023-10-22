
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AllianceMilitaryAi
{
    private Alliance _alliance;
    public List<List<Waypoint>> FrontlineWaypoints { get; private set; }
    public HashSet<Waypoint> FrontlineHash { get; private set; }
    public Dictionary<Regime, List<List<Waypoint>>> Fronts { get; private set; }

    public AllianceMilitaryAi(Alliance alliance)
    {
        _alliance = alliance;
        FrontlineWaypoints = new List<List<Waypoint>>();
        FrontlineHash = new HashSet<Waypoint>();
    }
    public void Calculate(Data data, Alliance alliance, 
        AllianceMajorTurnOrders orders)
    {
        if (data.HostLogicData.Context.ControlledAreas.ContainsKey(alliance) == false)
        {
            GD.Print("no control areas for alliance at poly " 
                     + alliance.Leader.Entity(data).GetPolys(data).First().Id);
            return;
        }
        var controlled = 
            data.HostLogicData.Context.ControlledAreas[alliance];
        
        CalculateFrontlines(controlled, orders, data);
    }

    private void CalculateFrontlines(IEnumerable<Waypoint> controlled, 
        AllianceMajorTurnOrders orders, Data d)
    {
        var forceBalances = d.HostLogicData.Context.WaypointForceBalances;
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

    private void ExpandFrontsAndFillGaps()
    {
        
    }
}